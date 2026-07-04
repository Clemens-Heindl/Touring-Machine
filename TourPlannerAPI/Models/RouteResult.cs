namespace TourPlannerAPI.Models;

public class RouteResult
{
    public string RouteGeoJson { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public int EstimatedMinutes { get; set; }

    // Elevation analysis derived from the 3D route geometry.
    public double AscentM { get; set; }
    public double DescentM { get; set; }
    public double MinElevationM { get; set; }
    public double MaxElevationM { get; set; }
    public List<ElevationPoint> ElevationProfile { get; set; } = new();
}

public class ElevationPoint
{
    public double DistanceKm { get; set; }
    public double ElevationM { get; set; }
}
