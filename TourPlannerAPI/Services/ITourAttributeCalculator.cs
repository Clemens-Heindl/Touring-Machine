using TourPlannerAPI.Models;

namespace TourPlannerAPI.Services;

/// <summary>Derives the computed attributes of a tour from its logs and route.</summary>
public interface ITourAttributeCalculator
{
    /// <summary>"New", "Known" or "Popular" based on the number of logs.</summary>
    string GetPopularity(Tour tour);

    /// <summary>"Child-friendly", "Moderate" or "Challenging" from difficulty/time/distance.</summary>
    string GetChildFriendliness(Tour tour);
}
