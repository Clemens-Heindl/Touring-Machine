using TourPlannerAPI.Dtos;

namespace TourPlannerAPI.Services;

/// <summary>Business-layer operations for users and authentication.</summary>
public interface IUserService
{
    Task<UserDto> RegisterAsync(RegisterRequest request);
    Task<UserDto?> LoginAsync(string email, string password);
    Task<UserDto> GetByIdAsync(int id);
}
