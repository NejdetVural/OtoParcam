using System.Text;
using OtoParcam.Application.Reports;
using OtoParcam.Domain.Enums;
using OtoParcam.Infrastructure.Services;
using static OtoParcam.Infrastructure.Tests.TestFixtures;

namespace OtoParcam.Infrastructure.Tests.Services;

public class ReportServiceTests
{
    [Fact]
    public async Task GetStatisticsReportDataAsync_CountsProductsByStatus()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);

        context.Products.AddRange(
            CreateProduct(category, model, ProductStatus.Available),
            CreateProduct(category, model, ProductStatus.Available),
            CreateProduct(category, model, ProductStatus.Hidden),
            CreateProduct(category, model, ProductStatus.Sold));
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var data = await service.GetStatisticsReportDataAsync();

        Assert.Equal(4, data.TotalProducts);
        Assert.Equal(2, data.AvailableProducts);
        Assert.Equal(1, data.HiddenProducts);
        Assert.Equal(1, data.SoldProducts);
    }

    [Fact]
    public async Task GetStatisticsReportDataAsync_ComputesRevenueCostAndProfitForSoldItems()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);

        var soldWithNegotiation = CreateProduct(category, model, ProductStatus.Sold, acquisitionCost: 800m, acquisitionSource: "ABC Hurdacılık");
        var soldWithoutNegotiation = CreateProduct(category, model, ProductStatus.Sold, acquisitionCost: 950m, acquisitionSource: "Sigorta - XYZ");
        context.Products.AddRange(soldWithNegotiation, soldWithoutNegotiation);
        CreateApprovedSale(context, soldWithNegotiation, originalPrice: 1000m, negotiatedPrice: 1500m);
        CreateApprovedSale(context, soldWithoutNegotiation, originalPrice: 1200m, negotiatedPrice: null);
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var data = await service.GetStatisticsReportDataAsync();

        Assert.Equal(2, data.SoldItems.Count);
        Assert.Equal(1500m + 1200m, data.TotalRevenue);
        Assert.Equal(800m + 950m, data.TotalAcquisitionCost);
        Assert.Equal((1500m - 800m) + (1200m - 950m), data.TotalProfit);

        var negotiatedRow = Assert.Single(data.SoldItems, r => r.Source == "ABC Hurdacılık");
        Assert.Equal(1500m, negotiatedRow.SoldPrice);
        Assert.Equal(700m, negotiatedRow.Profit);
    }

    [Fact]
    public async Task GetStatisticsReportDataAsync_ComputesInventoryValueFromAvailableProductsOnly()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);

        context.Products.AddRange(
            CreateProduct(category, model, ProductStatus.Available, price: 1000m, acquisitionCost: 600m),
            CreateProduct(category, model, ProductStatus.Available, price: 2000m, acquisitionCost: 1200m),
            CreateProduct(category, model, ProductStatus.Sold, price: 5000m, acquisitionCost: 3000m));
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var data = await service.GetStatisticsReportDataAsync();

        Assert.Equal(3000m, data.InventoryListValue);
        Assert.Equal(1800m, data.InventoryAcquisitionCost);
    }

    [Fact]
    public async Task GetStatisticsReportDataAsync_DailyPeriod_ExcludesSaleFromOverADayAgo()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);

        var recent = CreateProduct(category, model, ProductStatus.Sold);
        recent.SoldPrice = 500m;
        recent.SoldAt = DateTime.UtcNow.AddHours(-2);

        var old = CreateProduct(category, model, ProductStatus.Sold);
        old.SoldPrice = 400m;
        old.SoldAt = DateTime.UtcNow.AddHours(-30);

        context.Products.AddRange(recent, old);
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var data = await service.GetStatisticsReportDataAsync(ReportPeriod.Daily);

        var row = Assert.Single(data.SoldItems);
        Assert.Equal(500m, row.SoldPrice);
        Assert.Equal(1, data.SoldProducts);
    }

    [Fact]
    public async Task GetStatisticsReportDataAsync_WeeklyPeriod_IncludesWithinSevenDays_ExcludesOlder()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);

        var withinWeek = CreateProduct(category, model, ProductStatus.Sold);
        withinWeek.SoldPrice = 500m;
        withinWeek.SoldAt = DateTime.UtcNow.AddDays(-3);

        var older = CreateProduct(category, model, ProductStatus.Sold);
        older.SoldPrice = 400m;
        older.SoldAt = DateTime.UtcNow.AddDays(-10);

        context.Products.AddRange(withinWeek, older);
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var data = await service.GetStatisticsReportDataAsync(ReportPeriod.Weekly);

        var row = Assert.Single(data.SoldItems);
        Assert.Equal(500m, row.SoldPrice);
    }

    [Fact]
    public async Task GetStatisticsReportDataAsync_MonthlyPeriod_IncludesWithinThirtyDays_ExcludesOlder()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);

        var withinMonth = CreateProduct(category, model, ProductStatus.Sold);
        withinMonth.SoldPrice = 500m;
        withinMonth.SoldAt = DateTime.UtcNow.AddDays(-20);

        var older = CreateProduct(category, model, ProductStatus.Sold);
        older.SoldPrice = 400m;
        older.SoldAt = DateTime.UtcNow.AddDays(-40);

        context.Products.AddRange(withinMonth, older);
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var data = await service.GetStatisticsReportDataAsync(ReportPeriod.Monthly);

        var row = Assert.Single(data.SoldItems);
        Assert.Equal(500m, row.SoldPrice);
    }

    [Fact]
    public async Task GetStatisticsReportDataAsync_AllTime_IncludesSalesRegardlessOfAge()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);

        var veryOld = CreateProduct(category, model, ProductStatus.Sold);
        veryOld.SoldPrice = 400m;
        veryOld.SoldAt = DateTime.UtcNow.AddYears(-2);

        context.Products.Add(veryOld);
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var data = await service.GetStatisticsReportDataAsync(ReportPeriod.AllTime);

        Assert.Single(data.SoldItems);
        Assert.Equal(ReportPeriod.AllTime, data.Period);
    }

    [Fact]
    public async Task GetStatisticsReportDataAsync_SplitsBatchCostAcrossAllLinkedPartsRegardlessOfStatus()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var batch = CreateBatch("Ford Focus - sigorta hasarlı lotu", totalCost: 1000m);
        context.AcquisitionBatches.Add(batch);

        var sold = CreateProduct(category, model, ProductStatus.Sold, acquisitionBatch: batch);
        context.Products.AddRange(
            sold,
            CreateProduct(category, model, ProductStatus.Available, acquisitionBatch: batch),
            CreateProduct(category, model, ProductStatus.Available, acquisitionBatch: batch),
            CreateProduct(category, model, ProductStatus.Hidden, acquisitionBatch: batch));
        CreateApprovedSale(context, sold, originalPrice: 600m, negotiatedPrice: null);
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var data = await service.GetStatisticsReportDataAsync();

        var soldRow = Assert.Single(data.SoldItems);
        Assert.Equal(250m, soldRow.AcquisitionCost);
        Assert.Equal(350m, soldRow.Profit);

        var batchRow = Assert.Single(data.AcquisitionBatches);
        Assert.Equal(4, batchRow.PartCount);
        Assert.Equal(1, batchRow.SoldCount);
        Assert.Equal(2, batchRow.AvailableCount);
        Assert.Equal(1, batchRow.HiddenCount);
        Assert.Equal(600m, batchRow.RevenueSoFar);
        Assert.Equal(600m - 1000m, batchRow.ProfitSoFar);
    }

    [Fact]
    public async Task GetStatisticsReportDataAsync_PerPartOverrideWinsOverBatchSplit()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        var batch = CreateBatch("Ford Focus - sigorta hasarlı lotu", totalCost: 1000m);
        context.AcquisitionBatches.Add(batch);

        var sold = CreateProduct(category, model, ProductStatus.Sold, acquisitionCost: 300m, acquisitionBatch: batch);
        context.Products.AddRange(sold, CreateProduct(category, model, ProductStatus.Available, acquisitionBatch: batch));
        CreateApprovedSale(context, sold, originalPrice: 500m, negotiatedPrice: null);
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var data = await service.GetStatisticsReportDataAsync();

        var soldRow = Assert.Single(data.SoldItems);
        Assert.Equal(300m, soldRow.AcquisitionCost);
        Assert.Equal(200m, soldRow.Profit);
    }

    [Fact]
    public async Task GetStatisticsReportDataAsync_CountsManuallyMarkedSoldProduct_WithoutAnyPurchaseRequest()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);

        // Simulates the admin "mark as sold" (walk-in/offline sale) path — no PurchaseRequest exists at all.
        var manuallySold = CreateProduct(category, model, ProductStatus.Sold, acquisitionCost: 300m);
        manuallySold.SoldPrice = 700m;
        context.Products.Add(manuallySold);
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var data = await service.GetStatisticsReportDataAsync();

        var row = Assert.Single(data.SoldItems);
        Assert.Equal(700m, row.SoldPrice);
        Assert.Equal(400m, row.Profit);
        Assert.Equal(700m, data.TotalRevenue);
    }

    [Fact]
    public async Task GenerateStatisticsReportPdfAsync_ProducesAValidPdfDocument()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);

        var sold = CreateProduct(category, model, ProductStatus.Sold, acquisitionCost: 800m, acquisitionSource: "ABC Hurdacılık");
        context.Products.Add(sold);
        context.Products.Add(CreateProduct(category, model, ProductStatus.Available, price: 1000m));
        CreateApprovedSale(context, sold, originalPrice: 1000m, negotiatedPrice: 1500m);
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var pdfBytes = await service.GenerateStatisticsReportPdfAsync();

        Assert.NotEmpty(pdfBytes);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdfBytes, 0, 4));
    }

    [Fact]
    public async Task GenerateStatisticsReportPdfAsync_WorksWithNoSoldItems()
    {
        await using var context = CreateContext();
        var (category, model) = SeedCatalog(context);
        context.Products.Add(CreateProduct(category, model, ProductStatus.Available));
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var pdfBytes = await service.GenerateStatisticsReportPdfAsync();

        Assert.NotEmpty(pdfBytes);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdfBytes, 0, 4));
    }
}
