using Microsoft.AspNetCore.Mvc;
using TourPlannerAPI.Dtos;
using TourPlannerAPI.Services;

namespace TourPlannerAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class TourLogsController : ControllerBase
    {
        private readonly ITourLogService _tourLogService;

        public TourLogsController(ITourLogService tourLogService)
        {
            _tourLogService = tourLogService;
        }

        // GET: api/tours/5/logs
        [HttpGet("tours/{tourId}/logs")]
        public async Task<ActionResult<IReadOnlyList<TourLogDto>>> GetTourLogs(int tourId)
        {
            return Ok(await _tourLogService.GetByTourAsync(tourId));
        }

        // POST: api/tours/5/logs
        [HttpPost("tours/{tourId}/logs")]
        public async Task<ActionResult<TourLogDto>> PostTourLog(int tourId, SaveTourLogRequest request)
        {
            var created = await _tourLogService.CreateAsync(tourId, request);
            return CreatedAtAction(nameof(GetTourLogs), new { tourId = created.TourId }, created);
        }

        // PUT: api/logs/5
        [HttpPut("logs/{id}")]
        public async Task<ActionResult<TourLogDto>> PutTourLog(int id, SaveTourLogRequest request)
        {
            return Ok(await _tourLogService.UpdateAsync(id, request));
        }

        // DELETE: api/logs/5
        [HttpDelete("logs/{id}")]
        public async Task<IActionResult> DeleteTourLog(int id)
        {
            await _tourLogService.DeleteAsync(id);
            return NoContent();
        }
    }
}
