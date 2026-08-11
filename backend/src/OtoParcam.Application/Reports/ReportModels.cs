namespace OtoParcam.Application.Reports;

// Scopes the Sales Performance section (and its revenue/cost/profit totals) to a rolling window ending now, based on
// Product.SoldAt. AllTime (the default) preserves the original unscoped behavior — every other report section
// (Genel Özet counts, inventory value, acquisition batch part counts) always reflects current state, not the period.
public enum ReportPeriod
{
    AllTime,
    Daily,
    Weekly,
    Monthly
}

public class StatisticsReportData
{
    public DateTime GeneratedAt { get; set; }
    public ReportPeriod Period { get; set; }
    public int TotalProducts { get; set; }
    public int AvailableProducts { get; set; }
    public int SoldProducts { get; set; }
    public int HiddenProducts { get; set; }
    public int TotalCustomers { get; set; }
    public int PendingPurchaseRequests { get; set; }
    public IReadOnlyList<SoldProductReportRow> SoldItems { get; set; } = Array.Empty<SoldProductReportRow>();
    public decimal TotalRevenue { get; set; }
    public decimal TotalAcquisitionCost { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal InventoryListValue { get; set; }
    public decimal InventoryAcquisitionCost { get; set; }
    public IReadOnlyList<AcquisitionBatchReportRow> AcquisitionBatches { get; set; } = Array.Empty<AcquisitionBatchReportRow>();
}

public class SoldProductReportRow
{
    public string Title { get; set; } = string.Empty;
    public decimal? AcquisitionCost { get; set; }
    public decimal? SoldPrice { get; set; }
    public decimal? Profit { get; set; }
    public string? Source { get; set; }
}

public class AcquisitionBatchReportRow
{
    public string Source { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public decimal TotalCost { get; set; }
    public int PartCount { get; set; }
    public int AvailableCount { get; set; }
    public int SoldCount { get; set; }
    public int HiddenCount { get; set; }
    public decimal RevenueSoFar { get; set; }
    public decimal ProfitSoFar { get; set; }
}
