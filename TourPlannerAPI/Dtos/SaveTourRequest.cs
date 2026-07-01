using System.ComponentModel.DataAnnotations;

namespace TourPlannerAPI.Dtos;

/// <summary>Request shape for creating or updating a tour.</summary>
public class SaveTourRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string From { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string To { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string TransportType { get; set; } = string.Empty;

    public double Distance { get; set; }

    public TimeSpan EstimatedTime { get; set; }

    public string? RouteInformation { get; set; }

    // Temporary: the owning user is taken from the request until JWT auth lands
    // (Commit 7), after which it is derived from the authenticated identity.
    public int UserId { get; set; }
}
