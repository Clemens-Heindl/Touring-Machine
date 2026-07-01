namespace TourPlannerAPI.Dtos;

/// <summary>Response shape for a tour. Entities are never returned directly.</summary>
public class TourDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string TransportType { get; set; } = string.Empty;
    public double Distance { get; set; }
    public TimeSpan EstimatedTime { get; set; }
    public string? RouteInformation { get; set; }
    public List<TourLogDto> Logs { get; set; } = new();
}
