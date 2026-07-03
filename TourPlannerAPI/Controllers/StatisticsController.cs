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

    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    // GET: api/Statistics
    [HttpGet]
    public async Task<ActionResult<StatisticsDto>> GetStatistics()
    {
        return Ok(await _statisticsService.GetForUserAsync(User.GetUserId()));
    }
}
