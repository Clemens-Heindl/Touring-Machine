namespace TourPlannerAPI.Dtos;

/// <summary>Response shape for a tour log.</summary>
public class TourLogDto
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public string? Comment { get; set; }
    public int Difficulty { get; set; }
    public double TotalDistance { get; set; }
    public TimeSpan TotalTime { get; set; }
    public int Rating { get; set; }
    public int TourId { get; set; }
}
