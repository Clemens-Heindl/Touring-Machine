namespace TourPlannerAPI.Configuration;

/// <summary>
/// Thresholds for the computed tour attributes (popularity, child-friendliness).
/// Bound from the "TourAttributes" configuration section so the rules are not
/// hard-coded. Mirrors the frontend tour.config.ts values.
/// </summary>
public class ComputedAttributeOptions
{
    public const string SectionName = "TourAttributes";

    public PopularityOptions Popularity { get; set; } = new();
    public ChildFriendlinessOptions ChildFriendliness { get; set; } = new();
}

public class PopularityOptions
{
    /// <summary>Fewer logs than this => "New".</summary>
    public int KnownMinLogs { get; set; } = 1;

    /// <summary>At least this many logs => "Popular".</summary>
    public int PopularMinLogs { get; set; } = 3;
}

public class ChildFriendlinessOptions
{
    // Tours WITH logs: averages across all logs; all three must hold for a tier.
    public double FriendlyMaxAvgDifficulty { get; set; } = 2;
    public double FriendlyMaxAvgDistanceKm { get; set; } = 12;
    public double FriendlyMaxAvgTimeHours { get; set; } = 3;

    public double ModerateMaxAvgDifficulty { get; set; } = 3;
    public double ModerateMaxAvgDistanceKm { get; set; } = 25;
    public double ModerateMaxAvgTimeHours { get; set; } = 6;

    // Tours WITHOUT logs: derived from the tour's own distance and estimated time.
    public double NoLogsFriendlyMaxDistanceKm { get; set; } = 10;
    public double NoLogsFriendlyMaxTimeHours { get; set; } = 2;

    public double NoLogsModerateMaxDistanceKm { get; set; } = 25;
    public double NoLogsModerateMaxTimeHours { get; set; } = 5;
}
