using TourPlannerAPI.Dtos;

namespace TourPlannerAPI.Services;

/// <summary>Computes aggregate statistics over a user's tours and logs.</summary>
public interface IStatisticsService
{
    Task<StatisticsDto> GetForUserAsync(int userId);
}
