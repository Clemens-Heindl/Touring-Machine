namespace TourPlannerAPI.Dtos;

/// <summary>Response shape for a user. Never exposes the password hash.</summary>
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
