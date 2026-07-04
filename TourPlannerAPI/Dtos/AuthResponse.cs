namespace TourPlannerAPI.Dtos;

/// <summary>Response returned on successful registration and login.</summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}
