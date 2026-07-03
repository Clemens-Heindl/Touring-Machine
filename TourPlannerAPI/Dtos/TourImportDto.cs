namespace TourPlannerAPI.Dtos;

/// <summary>
/// Import shape for a tour and its logs. Compatible with the exported TourDto
/// JSON (extra fields such as ids and computed attributes are ignored on import);
/// imported tours are always assigned to the importing user with fresh ids.
/// </summary>
public class TourImportDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string TransportType { get; set; } = string.Empty;
    public double Distance { get; set; }
    public TimeSpan EstimatedTime { get; set; }
    public string? RouteInformation { get; set; }
    public string? ImageFileName { get; set; }
    public List<TourLogImportDto> Logs { get; set; } = new();
}

public class TourLogImportDto
{
    public DateTime DateTime { get; set; }
    public string? Comment { get; set; }
    public int Difficulty { get; set; }
    public double TotalDistance { get; set; }
    public TimeSpan TotalTime { get; set; }
    public int Rating { get; set; }
}
