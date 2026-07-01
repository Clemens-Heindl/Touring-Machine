using System.ComponentModel.DataAnnotations;

namespace TourPlannerAPI.Dtos;

/// <summary>Request shape for creating or updating a tour log.</summary>
public class SaveTourLogRequest
{
    [Required]
    public DateTime DateTime { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }

    [Range(1, 5)]
    public int Difficulty { get; set; }

    public double TotalDistance { get; set; }

    public TimeSpan TotalTime { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }
}
