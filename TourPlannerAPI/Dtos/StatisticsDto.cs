namespace TourPlannerAPI.Dtos;

/// <summary>Aggregated statistics for a single user's tours and logs.</summary>
public class StatisticsDto
{
    public int TourCount { get; set; }
    public int LogCount { get; set; }

    public double TotalTourDistanceKm { get; set; }
    public double TotalLoggedDistanceKm { get; set; }
    public double TotalLoggedTimeHours { get; set; }

    public double AverageRating { get; set; }
    public double AverageDifficulty { get; set; }

    public List<TransportTypeStat> ByTransportType { get; set; } = new();
    public List<MonthlyActivity> ActivityByMonth { get; set; } = new();

    public TourSummary? MostPopularTour { get; set; }
    public TourSummary? MostChallengingTour { get; set; }
}

public class TransportTypeStat
{
    public string TransportType { get; set; } = string.Empty;
    public int TourCount { get; set; }
}

public class MonthlyActivity
{
    /// <summary>Year-month bucket, e.g. "2026-04".</summary>
    public string Month { get; set; } = string.Empty;
    public int LogCount { get; set; }
    public double DistanceKm { get; set; }
}

public class TourSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>Human-readable label for what makes this tour notable.</summary>
    public string Detail { get; set; } = string.Empty;
}
