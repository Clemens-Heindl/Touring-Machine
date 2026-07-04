using TourPlannerAPI.Dtos;

namespace TourPlannerAPI.Services;

/// <summary>Business-layer operations for tour logs, scoped to the owning user.</summary>
public interface ITourLogService
{
    Task<IReadOnlyList<TourLogDto>> GetByTourAsync(int tourId, int userId);
    Task<TourLogDto> CreateAsync(int tourId, SaveTourLogRequest request, int userId);
    Task<TourLogDto> UpdateAsync(int logId, SaveTourLogRequest request, int userId);
    Task DeleteAsync(int logId, int userId);
}
