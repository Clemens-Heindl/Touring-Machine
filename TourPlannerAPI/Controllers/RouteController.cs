using Microsoft.AspNetCore.Mvc;
using TourPlannerAPI.Services;

namespace TourPlannerAPI.Controllers;

[ApiController]
[Route("api/route")]
public class RouteController : ControllerBase
{
    private readonly IRouteService _routeService;
    private readonly ILogger<RouteController> _logger;

    public RouteController(IRouteService routeService, ILogger<RouteController> logger)
    {
        _routeService = routeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoute(
        [FromQuery] string from,
        [FromQuery] string to,
        [FromQuery] string transportType)
    {
        if (string.IsNullOrWhiteSpace(from) ||
            string.IsNullOrWhiteSpace(to) ||
            string.IsNullOrWhiteSpace(transportType))
        {
            return BadRequest("Query parameters 'from', 'to', and 'transportType' are required.");
        }

        var result = await _routeService.GetRouteAsync(from, to, transportType);

        if (result is null)
        {
            _logger.LogWarning(
                "Route not found for {From} → {To} ({TransportType})",
                from, to, transportType);
            return NotFound("Could not calculate route for the given locations.");
        }

        return Ok(result);
    }
}
