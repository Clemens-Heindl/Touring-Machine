using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourPlannerAPI.Dtos;
using TourPlannerAPI.Services;
using TourPlannerAPI.Utilities;

namespace TourPlannerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;
    private readonly IReportService _reportService;

    public StatisticsController(IStatisticsService statisticsService, IReportService reportService)
    {
        _statisticsService = statisticsService;
        _reportService = reportService;
    }

    // GET: api/Statistics
    [HttpGet]
    public async Task<ActionResult<StatisticsDto>> GetStatistics()
    {
        return Ok(await _statisticsService.GetForUserAsync(User.GetUserId()));
    }

    // GET: api/Statistics/report -> PDF summary
    [HttpGet("report")]
    public async Task<IActionResult> GetSummaryReport()
    {
        var pdf = await _reportService.GenerateSummaryReportAsync(User.GetUserId());
        return File(pdf, "application/pdf", "tour-summary-report.pdf");
    }
}
