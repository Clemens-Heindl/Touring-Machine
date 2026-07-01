using System.ComponentModel.DataAnnotations;

namespace TourPlannerAPI.Dtos;

/// <summary>Request shape for user registration.</summary>
public class RegisterRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(50)]
    public string Email { get; set; } = string.Empty;

    // Currently the SHA-256 hash computed by the client. Commit 6 moves hashing
    // server-side and this becomes the raw password.
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
}
