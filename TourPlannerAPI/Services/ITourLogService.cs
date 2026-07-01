using TourPlannerAPI.Dtos;

namespace TourPlannerAPI.Services;

/// <summary>Business-layer operations for tour logs.</summary>
public interface ITourLogService
{
    Task<IReadOnlyList<TourLogDto>> GetByTourAsync(int tourId);
    Task<TourLogDto> CreateAsync(int tourId, SaveTourLogRequest request);
    Task<TourLogDto> UpdateAsync(int logId, SaveTourLogRequest request);
    Task DeleteAsync(int logId);
}
