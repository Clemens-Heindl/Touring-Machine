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
    public string? ImageFileName { get; set; }

    // Computed, read-only attributes derived by the business layer.
    public string Popularity { get; set; } = string.Empty;
    public string ChildFriendliness { get; set; } = string.Empty;

    public List<TourLogDto> Logs { get; set; } = new();
}
