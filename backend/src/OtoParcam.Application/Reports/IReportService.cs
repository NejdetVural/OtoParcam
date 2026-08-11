namespace OtoParcam.Application.Reports;

public interface IReportService
{
    Task<StatisticsReportData> GetStatisticsReportDataAsync(ReportPeriod period = ReportPeriod.AllTime, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateStatisticsReportPdfAsync(ReportPeriod period = ReportPeriod.AllTime, CancellationToken cancellationToken = default);
}
