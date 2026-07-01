using System.ComponentModel.DataAnnotations;

namespace TourPlannerAPI.Dtos;

/// <summary>Request shape for user self-registration.</summary>
public class RegisterRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(50)]
    public string Email { get; set; } = string.Empty;

    // Raw password sent over HTTPS; hashed server-side with BCrypt.
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
