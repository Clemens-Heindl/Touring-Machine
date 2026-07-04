using System.ComponentModel.DataAnnotations;

namespace TourPlannerAPI.Dtos;

/// <summary>Request shape for login. Credentials travel in the POST body, not the URL.</summary>
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
