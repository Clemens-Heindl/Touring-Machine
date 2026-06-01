using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourPlannerAPI.Data;
using TourPlannerAPI.Models;

namespace TourPlannerAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class TourLogsController : ControllerBase
    {
        private readonly TourPlannerDbContext _context;

        public TourLogsController(TourPlannerDbContext context)
        {
            _context = context;
        }

        // GET: api/tours/5/logs
        [HttpGet("tours/{tourId}/logs")]
        public async Task<ActionResult<IEnumerable<TourLog>>> GetTourLogs(int tourId)
        {
            var tour = await _context.Tours.Include(t => t.Logs).FirstOrDefaultAsync(t => t.Id == tourId);

            if (tour == null)
            {
                return NotFound("Tour not found.");
            }

            return Ok(tour.Logs);
        }

        // POST: api/tours/5/logs
        [HttpPost("tours/{tourId}/logs")]
        public async Task<ActionResult<TourLog>> PostTourLog(int tourId, TourLog tourLog)
        {
            var tour = await _context.Tours.FindAsync(tourId);
            if (tour == null)
            {
                return NotFound("Tour not found.");
            }

            tourLog.TourId = tourId;
            _context.TourLogs.Add(tourLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTourLogs), new { tourId = tourLog.TourId }, tourLog);
        }

        // PUT: api/logs/5
        [HttpPut("logs/{id}")]
        public async Task<IActionResult> PutTourLog(int id, TourLog tourLog)
        {
            if (id != tourLog.Id)
            {
                return BadRequest();
            }

            _context.Entry(tourLog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TourLogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/logs/5
        [HttpDelete("logs/{id}")]
        public async Task<IActionResult> DeleteTourLog(int id)
        {
            var tourLog = await _context.TourLogs.FindAsync(id);
            if (tourLog == null)
            {
                return NotFound();
            }

            _context.TourLogs.Remove(tourLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TourLogExists(int id)
        {
            return _context.TourLogs.Any(e => e.Id == id);
        }
    }
}
