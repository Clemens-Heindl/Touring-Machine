using Microsoft.Extensions.Options;
using TourPlannerAPI.Configuration;
using TourPlannerAPI.Models;

namespace TourPlannerAPI.Services;

/// <summary>
/// Config-driven implementation of the computed tour attributes. Kept as its own
/// business-layer component so the rules are reusable (list, search, statistics)
/// and unit-testable in isolation.
/// </summary>
public class TourAttributeCalculator : ITourAttributeCalculator
{
    public const string PopularityNew = "New";
    public const string PopularityKnown = "Known";
    public const string PopularityPopular = "Popular";

    public const string ChildFriendly = "Child-friendly";
    public const string Moderate = "Moderate";
    public const string Challenging = "Challenging";

    private readonly ComputedAttributeOptions _options;

    public TourAttributeCalculator(IOptions<ComputedAttributeOptions> options)
    {
        _options = options.Value;
    }

    public string GetPopularity(Tour tour)
    {
        var count = tour.Logs.Count;
        if (count < _options.Popularity.KnownMinLogs) return PopularityNew;
        if (count < _options.Popularity.PopularMinLogs) return PopularityKnown;
        return PopularityPopular;
    }

    public string GetChildFriendliness(Tour tour)
    {
        var cfg = _options.ChildFriendliness;

        if (tour.Logs.Count == 0)
        {
            var hours = tour.EstimatedTime.TotalHours;
            if (tour.Distance <= cfg.NoLogsFriendlyMaxDistanceKm && hours <= cfg.NoLogsFriendlyMaxTimeHours)
                return ChildFriendly;
            if (tour.Distance <= cfg.NoLogsModerateMaxDistanceKm && hours <= cfg.NoLogsModerateMaxTimeHours)
                return Moderate;
            return Challenging;
        }

        var avgDifficulty = tour.Logs.Average(l => l.Difficulty);
        var avgDistance = tour.Logs.Average(l => l.TotalDistance);
        var avgTimeHours = tour.Logs.Average(l => l.TotalTime.TotalHours);

        if (avgDifficulty <= cfg.FriendlyMaxAvgDifficulty &&
            avgDistance <= cfg.FriendlyMaxAvgDistanceKm &&
            avgTimeHours <= cfg.FriendlyMaxAvgTimeHours)
            return ChildFriendly;

        if (avgDifficulty <= cfg.ModerateMaxAvgDifficulty &&
            avgDistance <= cfg.ModerateMaxAvgDistanceKm &&
            avgTimeHours <= cfg.ModerateMaxAvgTimeHours)
            return Moderate;

        return Challenging;
    }
}
