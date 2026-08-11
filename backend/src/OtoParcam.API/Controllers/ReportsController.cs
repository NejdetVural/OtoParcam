using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtoParcam.Application.Reports;
using OtoParcam.Domain.Constants;

namespace OtoParcam.API.Controllers;

[ApiController]
[Route("api/v1/admin/reports")]
[Authorize(Roles = Roles.Administrator)]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatisticsReport([FromQuery] ReportPeriod period, CancellationToken cancellationToken)
    {
        var pdf = await _reportService.GenerateStatisticsReportPdfAsync(period, cancellationToken);
        var fileName = $"otoparcam-rapor-{DateTime.UtcNow:yyyyMMdd-HHmm}.pdf";
        return File(pdf, "application/pdf", fileName);
    }
}
