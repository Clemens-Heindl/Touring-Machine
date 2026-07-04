using Microsoft.Extensions.Options;
using TourPlannerAPI.Configuration;
using TourPlannerAPI.Models;
using TourPlannerAPI.Services;

namespace TourPlannerAPI.Tests;

/// <summary>Shared builders and default options for the unit tests.</summary>
internal static class TestData
{
    public static IOptions<ComputedAttributeOptions> DefaultAttributeOptions()
        => Options.Create(new ComputedAttributeOptions());

    public static ITourAttributeCalculator Calculator()
        => new TourAttributeCalculator(DefaultAttributeOptions());

    public static Tour Tour(int id = 1, int userId = 1, params TourLog[] logs)
        => new()
        {
            Id = id,
            UserId = userId,
            Name = "Test Tour",
            From = "A",
            To = "B",
            TransportType = "Bike",
            Distance = 10,
            EstimatedTime = TimeSpan.FromHours(1),
            Logs = logs.ToList()
        };

    public static TourLog Log(int difficulty = 3, double distance = 10, double hours = 1, int rating = 3)
        => new()
        {
            Difficulty = difficulty,
            TotalDistance = distance,
            TotalTime = TimeSpan.FromHours(hours),
            Rating = rating,
            DateTime = new DateTime(2026, 4, 1)
        };
}
