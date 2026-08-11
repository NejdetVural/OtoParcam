using System.Globalization;
using Microsoft.EntityFrameworkCore;
using OtoParcam.Application.Reports;
using OtoParcam.Domain.Constants;
using OtoParcam.Domain.Entities;
using OtoParcam.Domain.Enums;
using OtoParcam.Infrastructure.Persistence;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OtoParcam.Infrastructure.Services;

public class ReportService : IReportService
{
    static ReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private readonly ApplicationDbContext _dbContext;

    public ReportService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StatisticsReportData> GetStatisticsReportDataAsync(ReportPeriod period = ReportPeriod.AllTime, CancellationToken cancellationToken = default)
    {
        var totalProducts = await _dbContext.Products.CountAsync(cancellationToken);
        var availableProducts = await _dbContext.Products.CountAsync(p => p.Status == ProductStatus.Available, cancellationToken);
        var hiddenProducts = await _dbContext.Products.CountAsync(p => p.Status == ProductStatus.Hidden, cancellationToken);

        var customerRoleId = await _dbContext.Roles
            .Where(r => r.Name == Roles.Customer)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);
        var totalCustomers = await _dbContext.UserRoles
            .CountAsync(ur => ur.RoleId == customerRoleId, cancellationToken);

        var pendingPurchaseRequests = await _dbContext.PurchaseRequests
            .CountAsync(r => r.Status == PurchaseRequestStatus.Pending, cancellationToken);

        // Sales Performance is the only section scoped to the requested period (via SoldAt, set independently of
        // UpdatedAt so a later unrelated edit to a sold product can't shift it into a different period). Every other
        // section reflects current state regardless of period.
        var since = PeriodStart(period);
        var soldProducts = await _dbContext.Products
            .Where(p => p.Status == ProductStatus.Sold && (since == null || p.SoldAt >= since))
            .Include(p => p.SourceVehicleModel).ThenInclude(m => m.VehicleBrand)
            .Include(p => p.AcquisitionBatch)
            .ToListAsync(cancellationToken);

        // Every part linked to an acquisition batch, regardless of status — needed to split
        // the batch's lump-sum cost evenly across all parts it produced (BR: batches are bought
        // together but sold individually, so per-part cost is only known once we know the count).
        var batchedProducts = await _dbContext.Products
            .Where(p => p.AcquisitionBatchId != null)
            .Include(p => p.AcquisitionBatch)
            .ToListAsync(cancellationToken);

        var batchPartCounts = batchedProducts
            .GroupBy(p => p.AcquisitionBatchId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        decimal? EffectiveCost(Product p) => p.AcquisitionCost ?? SplitBatchCost(p.AcquisitionBatch, p.AcquisitionBatchId, batchPartCounts);

        var soldItems = soldProducts.Select(p =>
        {
            var cost = EffectiveCost(p);

            return new SoldProductReportRow
            {
                Title = BuildTitle(p.SourceVehicleModel),
                AcquisitionCost = cost,
                SoldPrice = p.SoldPrice,
                Profit = p.SoldPrice.HasValue && cost.HasValue ? p.SoldPrice - cost : null,
                Source = p.AcquisitionSource ?? p.AcquisitionBatch?.Source,
            };
        }).ToList();

        var availableProductEntities = await _dbContext.Products
            .Where(p => p.Status == ProductStatus.Available)
            .Include(p => p.AcquisitionBatch)
            .ToListAsync(cancellationToken);

        var acquisitionBatches = batchedProducts
            .GroupBy(p => p.AcquisitionBatch!)
            .Select(g =>
            {
                var batch = g.Key;
                var revenueSoFar = g
                    .Where(p => p.Status == ProductStatus.Sold)
                    .Sum(p => p.SoldPrice ?? 0);

                return new AcquisitionBatchReportRow
                {
                    Source = batch.Source,
                    PurchaseDate = batch.PurchaseDate,
                    TotalCost = batch.TotalCost,
                    PartCount = g.Count(),
                    AvailableCount = g.Count(p => p.Status == ProductStatus.Available),
                    SoldCount = g.Count(p => p.Status == ProductStatus.Sold),
                    HiddenCount = g.Count(p => p.Status == ProductStatus.Hidden),
                    RevenueSoFar = revenueSoFar,
                    ProfitSoFar = revenueSoFar - batch.TotalCost,
                };
            })
            .OrderByDescending(r => r.PurchaseDate)
            .ToList();

        return new StatisticsReportData
        {
            GeneratedAt = DateTime.UtcNow,
            Period = period,
            TotalProducts = totalProducts,
            AvailableProducts = availableProducts,
            SoldProducts = soldItems.Count,
            HiddenProducts = hiddenProducts,
            TotalCustomers = totalCustomers,
            PendingPurchaseRequests = pendingPurchaseRequests,
            SoldItems = soldItems,
            TotalRevenue = soldItems.Sum(i => i.SoldPrice ?? 0),
            TotalAcquisitionCost = soldItems.Sum(i => i.AcquisitionCost ?? 0),
            TotalProfit = soldItems.Sum(i => i.Profit ?? 0),
            InventoryListValue = availableProductEntities.Sum(p => p.Price ?? 0),
            InventoryAcquisitionCost = availableProductEntities.Sum(p => EffectiveCost(p) ?? 0),
            AcquisitionBatches = acquisitionBatches,
        };
    }

    private static DateTime? PeriodStart(ReportPeriod period) => period switch
    {
        ReportPeriod.Daily => DateTime.UtcNow.AddDays(-1),
        ReportPeriod.Weekly => DateTime.UtcNow.AddDays(-7),
        ReportPeriod.Monthly => DateTime.UtcNow.AddMonths(-1),
        _ => null,
    };

    private static string PeriodLabel(ReportPeriod period) => period switch
    {
        ReportPeriod.Daily => "Son 24 Saat",
        ReportPeriod.Weekly => "Son 7 Gün",
        ReportPeriod.Monthly => "Son 30 Gün",
        _ => "Tüm Zamanlar",
    };

    private static decimal? SplitBatchCost(AcquisitionBatch? batch, Guid? batchId, Dictionary<Guid, int> batchPartCounts)
    {
        if (batch is null || !batchId.HasValue || !batchPartCounts.TryGetValue(batchId.Value, out var count) || count == 0)
        {
            return null;
        }

        return batch.TotalCost / count;
    }

    public async Task<byte[]> GenerateStatisticsReportPdfAsync(ReportPeriod period = ReportPeriod.AllTime, CancellationToken cancellationToken = default)
    {
        var data = await GetStatisticsReportDataAsync(period, cancellationToken);
        return BuildPdf(data);
    }

    // Brand palette — kept independent of the frontend's exact Tailwind hex values (a PDF doesn't need pixel-parity,
    // just a consistent, professional look): slate for structure, blue for the brand accent, emerald/rose for profit/loss.
    private const string ColorPrimaryDark = "#1E293B";
    private const string ColorAccent = "#2563EB";
    private const string ColorMuted = "#64748B";
    private const string ColorBorder = "#E2E8F0";
    private const string ColorRowAlt = "#F8FAFC";
    private const string ColorCardBg = "#F1F5F9";
    private const string ColorProfit = "#059669";
    private const string ColorLoss = "#DC2626";

    private static byte[] BuildPdf(StatisticsReportData data)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(ColorPrimaryDark));

                page.Header().Element(c => ReportHeader(c, data));

                page.Content().Padding(30).Column(column =>
                {
                    column.Spacing(16);
                    column.Item().Element(c => ProfitHero(c, data));
                    column.Item().Element(c => SummarySection(c, data));
                    column.Item().Element(c => SalesSection(c, data));
                    column.Item().Element(c => AcquisitionBatchesSection(c, data));
                    column.Item().Element(c => InventorySection(c, data));
                });

                page.Footer().Padding(20).Column(col =>
                {
                    col.Item().BorderTop(0.5f).BorderColor(ColorBorder).PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem().Text("OtoParcam").FontSize(8).FontColor(ColorMuted);
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.DefaultTextStyle(s => s.FontSize(8).FontColor(ColorMuted));
                            x.Span("Sayfa ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                    });
                });
            });
        }).GeneratePdf();
    }

    private static void ReportHeader(IContainer container, StatisticsReportData data)
    {
        container.Background(ColorPrimaryDark).Padding(24).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("OtoParcam").FontSize(22).Bold().FontColor(Colors.White);
                col.Item().PaddingTop(2).Text("Satış ve Envanter Raporu").FontSize(11).FontColor(Colors.Grey.Lighten3);
            });
            row.AutoItem().AlignRight().Column(col =>
            {
                col.Item().AlignRight().Text("Oluşturulma Tarihi").FontSize(8).FontColor(Colors.Grey.Lighten3);
                col.Item().AlignRight().Text($"{data.GeneratedAt:dd.MM.yyyy HH:mm}").FontSize(11).Bold().FontColor(Colors.White);
                col.Item().PaddingTop(4).AlignRight().Text("Rapor Aralığı").FontSize(8).FontColor(Colors.Grey.Lighten3);
                col.Item().AlignRight().Text(PeriodLabel(data.Period)).FontSize(11).Bold().FontColor(Colors.White);
            });
        });
    }

    private static void ProfitHero(IContainer container, StatisticsReportData data)
    {
        var color = data.TotalProfit >= 0 ? ColorProfit : ColorLoss;
        container.Background(color).Padding(14).Row(row =>
        {
            row.RelativeItem().AlignMiddle().Column(col =>
            {
                col.Item().Text($"Toplam Kar ({PeriodLabel(data.Period)})").FontSize(9).FontColor(Colors.White);
                col.Item().PaddingTop(2).Text(data.TotalProfit >= 0 ? "Karda" : "Zararda").FontSize(9).Bold().FontColor(Colors.White);
            });
            row.AutoItem().AlignMiddle().Text(FormatCurrency(data.TotalProfit)).FontSize(20).Bold().FontColor(Colors.White);
        });
    }

    private static void SummarySection(IContainer container, StatisticsReportData data)
    {
        container.Column(col =>
        {
            SectionTitle(col, "Genel Özet");

            col.Item().PaddingTop(10).Row(row =>
            {
                row.Spacing(8);
                StatTile(row, "Toplam Ürün", data.TotalProducts.ToString());
                StatTile(row, "Satıştaki Ürün", data.AvailableProducts.ToString(), ColorProfit);
                StatTile(row, $"Satılan Ürün ({PeriodLabel(data.Period)})", data.SoldProducts.ToString());
            });
            col.Item().PaddingTop(8).Row(row =>
            {
                row.Spacing(8);
                StatTile(row, "Gizli Ürün", data.HiddenProducts.ToString());
                StatTile(row, "Toplam Müşteri", data.TotalCustomers.ToString());
                StatTile(row, "Bekleyen Talep", data.PendingPurchaseRequests.ToString(),
                    data.PendingPurchaseRequests > 0 ? ColorAccent : null);
            });
        });
    }

    private static void SalesSection(IContainer container, StatisticsReportData data)
    {
        container.Column(col =>
        {
            SectionTitle(col, $"Satış Performansı ({PeriodLabel(data.Period)})");

            if (data.SoldItems.Count == 0)
            {
                col.Item().PaddingTop(8).Element(EmptyStateBox).Text("Bu dönemde satılmış ürün yok.").FontColor(ColorMuted);
                return;
            }

            col.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Ürün");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Maliyet");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Satış Fiyatı");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Kar");
                    header.Cell().Element(HeaderCell).Text("Kaynak");
                });

                for (var i = 0; i < data.SoldItems.Count; i++)
                {
                    var item = data.SoldItems[i];
                    table.Cell().Element(c => BodyCell(c, i)).Text(item.Title);
                    table.Cell().Element(c => BodyCell(c, i)).AlignRight().Text(FormatCurrency(item.AcquisitionCost));
                    table.Cell().Element(c => BodyCell(c, i)).AlignRight().Text(FormatCurrency(item.SoldPrice));
                    table.Cell().Element(c => BodyCell(c, i)).AlignRight().Text(FormatCurrency(item.Profit)).FontColor(ProfitColor(item.Profit)).Bold();
                    table.Cell().Element(c => BodyCell(c, i)).Text(item.Source ?? "-").FontColor(ColorMuted);
                }
            });

            col.Item().PaddingTop(10).Row(row =>
            {
                row.Spacing(8);
                StatTile(row, "Toplam Gelir", FormatCurrency(data.TotalRevenue));
                StatTile(row, "Toplam Maliyet", FormatCurrency(data.TotalAcquisitionCost));
                StatTile(row, "Toplam Kar", FormatCurrency(data.TotalProfit), ProfitColor(data.TotalProfit));
            });
        });
    }

    private static void AcquisitionBatchesSection(IContainer container, StatisticsReportData data)
    {
        container.Column(col =>
        {
            SectionTitle(col, "Toplu Alımlar (Hasarlı/Sigorta Lotları)");

            if (data.AcquisitionBatches.Count == 0)
            {
                col.Item().PaddingTop(8).Element(EmptyStateBox).Text("Henüz toplu alım kaydı yok.").FontColor(ColorMuted);
                return;
            }

            col.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Kaynak");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Toplam Maliyet");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Parça (Satılan/Toplam)");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Gelir");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Kar/Zarar");
                });

                for (var i = 0; i < data.AcquisitionBatches.Count; i++)
                {
                    var batch = data.AcquisitionBatches[i];
                    table.Cell().Element(c => BodyCell(c, i)).Text(batch.Source);
                    table.Cell().Element(c => BodyCell(c, i)).AlignRight().Text(FormatCurrency(batch.TotalCost));
                    table.Cell().Element(c => BodyCell(c, i)).AlignRight().Text($"{batch.SoldCount}/{batch.PartCount}");
                    table.Cell().Element(c => BodyCell(c, i)).AlignRight().Text(FormatCurrency(batch.RevenueSoFar));
                    table.Cell().Element(c => BodyCell(c, i)).AlignRight().Text(FormatCurrency(batch.ProfitSoFar)).FontColor(ProfitColor(batch.ProfitSoFar)).Bold();
                }
            });
        });
    }

    private static void InventorySection(IContainer container, StatisticsReportData data)
    {
        var potentialProfit = data.InventoryListValue - data.InventoryAcquisitionCost;

        container.Column(col =>
        {
            SectionTitle(col, "Envanter Değeri (Satıştaki Ürünler)");

            col.Item().PaddingTop(10).Row(row =>
            {
                row.Spacing(8);
                StatTile(row, "Liste Fiyatı Toplamı", FormatCurrency(data.InventoryListValue));
                StatTile(row, "Maliyet Toplamı", FormatCurrency(data.InventoryAcquisitionCost));
                StatTile(row, "Potansiyel Kar", FormatCurrency(potentialProfit), ProfitColor(potentialProfit));
            });
        });
    }

    private static void SectionTitle(ColumnDescriptor column, string title)
    {
        column.Item().Row(row =>
        {
            row.ConstantItem(4).Height(16).Background(ColorAccent);
            row.RelativeItem().PaddingLeft(8).AlignMiddle().Text(title).FontSize(13).Bold().FontColor(ColorPrimaryDark);
        });
    }

    private static void StatTile(RowDescriptor row, string label, string value, string? accentColor = null)
    {
        row.RelativeItem().Background(ColorCardBg).Padding(10).Column(col =>
        {
            col.Item().Text(value).FontSize(15).Bold().FontColor(accentColor ?? ColorPrimaryDark);
            col.Item().PaddingTop(2).Text(label).FontSize(8).FontColor(ColorMuted);
        });
    }

    private static IContainer EmptyStateBox(IContainer container) =>
        container.Background(ColorRowAlt).Padding(12);

    private static IContainer HeaderCell(IContainer container) =>
        container.Background(ColorPrimaryDark).DefaultTextStyle(x => x.FontColor(Colors.White).Bold())
            .PaddingVertical(6).PaddingHorizontal(4);

    private static IContainer BodyCell(IContainer container, int rowIndex) =>
        container.Background(rowIndex % 2 == 0 ? Colors.White : ColorRowAlt)
            .PaddingVertical(6).PaddingHorizontal(4)
            .BorderBottom(0.5f).BorderColor(ColorBorder);

    private static string ProfitColor(decimal? value) =>
        value is null ? ColorMuted : value >= 0 ? ColorProfit : ColorLoss;

    private static string FormatCurrency(decimal? value) =>
        value.HasValue ? value.Value.ToString("N0", CultureInfo.GetCultureInfo("tr-TR")) + " ₺" : "-";

    private static string FormatCurrency(decimal value) => FormatCurrency((decimal?)value);

    private static string BuildTitle(VehicleModel vehicleModel)
    {
        var variantPart = string.IsNullOrWhiteSpace(vehicleModel.Variant) ? string.Empty : $" {vehicleModel.Variant}";
        return $"{vehicleModel.VehicleBrand.Name} {vehicleModel.Name}{variantPart} ({vehicleModel.StartYear}-{vehicleModel.EndYear})";
    }
}
