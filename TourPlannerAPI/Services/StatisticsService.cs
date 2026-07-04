using TourPlannerAPI.Dtos;
using TourPlannerAPI.Models;
using TourPlannerAPI.Repositories;

namespace TourPlannerAPI.Services;

/// <summary>Business logic for the statistics dashboard.</summary>
public class StatisticsService : IStatisticsService
{
    private readonly ITourRepository _tours;
    private readonly ITourAttributeCalculator _attributes;
    private readonly ILogger<StatisticsService> _logger;

    public StatisticsService(
        ITourRepository tours,
        ITourAttributeCalculator attributes,
        ILogger<StatisticsService> logger)
    {
        _tours = tours;
        _attributes = attributes;
        _logger = logger;
    }

    public async Task<StatisticsDto> GetForUserAsync(int userId)
    {
        var tours = await _tours.GetAllByUserAsync(userId);
        var logs = tours.SelectMany(t => t.Logs).ToList();

        var stats = new StatisticsDto
        {
            TourCount = tours.Count,
            LogCount = logs.Count,
            TotalTourDistanceKm = Math.Round(tours.Sum(t => t.Distance), 2),
            TotalLoggedDistanceKm = Math.Round(logs.Sum(l => l.TotalDistance), 2),
            TotalLoggedTimeHours = Math.Round(logs.Sum(l => l.TotalTime.TotalHours), 2),
            AverageRating = logs.Count > 0 ? Math.Round(logs.Average(l => l.Rating), 2) : 0,
            AverageDifficulty = logs.Count > 0 ? Math.Round(logs.Average(l => l.Difficulty), 2) : 0,
            ByTransportType = tours
                .GroupBy(t => t.TransportType)
                .Select(g => new TransportTypeStat { TransportType = g.Key, TourCount = g.Count() })
                .OrderByDescending(s => s.TourCount)
                .ToList(),
            ActivityByMonth = logs
                .GroupBy(l => l.DateTime.ToString("yyyy-MM"))
                .Select(g => new MonthlyActivity
                {
                    Month = g.Key,
                    LogCount = g.Count(),
                    DistanceKm = Math.Round(g.Sum(l => l.TotalDistance), 2)
                })
                .OrderBy(m => m.Month)
                .ToList(),
            MostPopularTour = BuildMostPopular(tours),
            MostChallengingTour = BuildMostChallenging(tours)
        };

        _logger.LogInformation("Computed statistics for user {UserId}: {Tours} tours, {Logs} logs",
            userId, stats.TourCount, stats.LogCount);
        return stats;
    }

    private TourSummary? BuildMostPopular(IReadOnlyList<Tour> tours)
    {
        var tour = tours
            .Where(t => t.Logs.Count > 0)
            .OrderByDescending(t => t.Logs.Count)
            .FirstOrDefault();

        return tour is null
            ? null
            : new TourSummary
            {
                Id = tour.Id,
                Name = tour.Name,
                Detail = $"{_attributes.GetPopularity(tour)} · {tour.Logs.Count} log(s)"
            };
    }

    private TourSummary? BuildMostChallenging(IReadOnlyList<Tour> tours)
    {
        var tour = tours
            .Where(t => t.Logs.Count > 0)
            .OrderByDescending(t => t.Logs.Average(l => l.Difficulty))
            .FirstOrDefault();

        return tour is null
            ? null
            : new TourSummary
            {
                Id = tour.Id,
                Name = tour.Name,
                Detail = $"{_attributes.GetChildFriendliness(tour)} · avg difficulty {tour.Logs.Average(l => l.Difficulty):0.0}"
            };
    }
}
