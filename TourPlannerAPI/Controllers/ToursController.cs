using Microsoft.AspNetCore.Mvc;
using TourPlannerAPI.Dtos;
using TourPlannerAPI.Services;

namespace TourPlannerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToursController : ControllerBase
    {
        private readonly ITourService _tourService;

        public ToursController(ITourService tourService)
        {
            _tourService = tourService;
        }

        // GET: api/Tours
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<TourDto>>> GetTours()
        {
            return Ok(await _tourService.GetAllAsync());
        }

        // GET: api/Tours/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TourDto>> GetTour(int id)
        {
            return Ok(await _tourService.GetByIdAsync(id));
        }

        // POST: api/Tours
        [HttpPost]
        public async Task<ActionResult<TourDto>> PostTour(SaveTourRequest request)
        {
            var created = await _tourService.CreateAsync(request);
            return CreatedAtAction(nameof(GetTour), new { id = created.Id }, created);
        }

        // PUT: api/Tours/5
        [HttpPut("{id}")]
        public async Task<ActionResult<TourDto>> PutTour(int id, SaveTourRequest request)
        {
            return Ok(await _tourService.UpdateAsync(id, request));
        }

        // DELETE: api/Tours/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTour(int id)
        {
            await _tourService.DeleteAsync(id);
            return NoContent();
        }
    }
}
