namespace TourPlannerAPI.Models;

public class RouteResult
{
    public string RouteGeoJson { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public int EstimatedMinutes { get; set; }
}
