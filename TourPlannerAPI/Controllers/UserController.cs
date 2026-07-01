using Microsoft.AspNetCore.Mvc;
using TourPlannerAPI.Dtos;
using TourPlannerAPI.Services;

namespace TourPlannerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            return Ok(await _userService.GetByIdAsync(id));
        }

        // POST: api/Users  (self-registration)
        [HttpPost]
        public async Task<ActionResult<UserDto>> Register(RegisterRequest request)
        {
            var created = await _userService.RegisterAsync(request);
            return CreatedAtAction(nameof(GetUser), new { id = created.Id }, created);
        }

        // GET: api/Users/login
        [HttpGet("login")]
        public async Task<ActionResult<UserDto>> Login(string email, string passwordHash)
        {
            var user = await _userService.LoginAsync(email, passwordHash);
            if (user is null)
            {
                return Unauthorized();
            }

            return Ok(user);
        }
    }
}
