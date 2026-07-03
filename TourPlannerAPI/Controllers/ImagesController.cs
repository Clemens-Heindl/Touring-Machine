using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourPlannerAPI.Services;

namespace TourPlannerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ImagesController : ControllerBase
{
    private readonly IImageStorageService _images;

    public ImagesController(IImageStorageService images)
    {
        _images = images;
    }

    // POST: api/Images  (multipart/form-data)
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<object>> Upload(IFormFile file)
    {
        var fileName = await _images.SaveAsync(file);
        return Ok(new { fileName });
    }

    // GET: api/Images/{fileName}  (public so <img> tags can load it)
    [HttpGet("{fileName}")]
    [AllowAnonymous]
    public IActionResult Get(string fileName)
    {
        var image = _images.Open(fileName);
        if (image is null)
        {
            return NotFound();
        }

        return File(image.Value.Stream, image.Value.ContentType);
    }
}
