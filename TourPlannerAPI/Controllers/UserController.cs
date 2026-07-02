using Microsoft.AspNetCore.Authorization;
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
        private readonly IJwtTokenService _jwtTokenService;

        public UsersController(IUserService userService, IJwtTokenService jwtTokenService)
        {
            _userService = userService;
            _jwtTokenService = jwtTokenService;
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            return Ok(await _userService.GetByIdAsync(id));
        }

        // POST: api/Users/register  (self-registration)
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            var created = await _userService.RegisterAsync(request);
            var token = _jwtTokenService.CreateToken(created);
            return CreatedAtAction(nameof(GetUser), new { id = created.Id },
                new AuthResponse { Token = token, User = created });
        }

        // POST: api/Users/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var user = await _userService.LoginAsync(request.Email, request.Password);
            if (user is null)
            {
                return Unauthorized();
            }

            var token = _jwtTokenService.CreateToken(user);
            return Ok(new AuthResponse { Token = token, User = user });
        }
    }
}
