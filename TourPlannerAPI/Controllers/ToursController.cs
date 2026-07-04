using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourPlannerAPI.Dtos;
using TourPlannerAPI.Services;
using TourPlannerAPI.Utilities;

namespace TourPlannerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ToursController : ControllerBase
    {
        private readonly ITourService _tourService;
        private readonly IReportService _reportService;

        public ToursController(ITourService tourService, IReportService reportService)
        {
            _tourService = tourService;
            _reportService = reportService;
        }

        // GET: api/Tours
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<TourDto>>> GetTours()
        {
            return Ok(await _tourService.GetAllForUserAsync(User.GetUserId()));
        }

        // GET: api/Tours/export  -> downloadable JSON bundle of the user's tours
        [HttpGet("export")]
        public async Task<IActionResult> ExportTours()
        {
            var tours = await _tourService.GetAllForUserAsync(User.GetUserId());
            var json = JsonSerializer.Serialize(tours, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
            var bytes = Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", "tours.json");
        }

        // POST: api/Tours/import  -> persist an imported JSON bundle for the user
        [HttpPost("import")]
        public async Task<ActionResult<IReadOnlyList<TourDto>>> ImportTours(List<TourImportDto> tours)
        {
            return Ok(await _tourService.ImportAsync(User.GetUserId(), tours));
        }

        // GET: api/Tours/search?q=...
        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyList<TourDto>>> SearchTours([FromQuery] string? q)
        {
            return Ok(await _tourService.SearchAsync(User.GetUserId(), q));
        }

        // GET: api/Tours/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TourDto>> GetTour(int id)
        {
            return Ok(await _tourService.GetByIdAsync(id, User.GetUserId()));
        }

        // POST: api/Tours
        [HttpPost]
        public async Task<ActionResult<TourDto>> PostTour(SaveTourRequest request)
        {
            var created = await _tourService.CreateAsync(request, User.GetUserId());
            return CreatedAtAction(nameof(GetTour), new { id = created.Id }, created);
        }

        // PUT: api/Tours/5
        [HttpPut("{id}")]
        public async Task<ActionResult<TourDto>> PutTour(int id, SaveTourRequest request)
        {
            return Ok(await _tourService.UpdateAsync(id, request, User.GetUserId()));
        }

        // DELETE: api/Tours/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTour(int id)
        {
            await _tourService.DeleteAsync(id, User.GetUserId());
            return NoContent();
        }

        // GET: api/Tours/5/report -> PDF
        [HttpGet("{id}/report")]
        public async Task<IActionResult> GetTourReport(int id)
        {
            var pdf = await _reportService.GenerateTourReportAsync(id, User.GetUserId());
            return File(pdf, "application/pdf", $"tour-{id}-report.pdf");
        }
    }
}
