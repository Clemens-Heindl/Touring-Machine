using TourPlannerAPI.Models;

namespace TourPlannerAPI.Repositories;

/// <summary>Data-access abstraction for tour logs.</summary>
public interface ITourLogRepository
{
    Task<IReadOnlyList<TourLog>> GetByTourIdAsync(int tourId);
    Task<TourLog?> GetByIdAsync(int id);
    Task<TourLog> AddAsync(TourLog log);
    Task UpdateAsync(TourLog log);
    Task DeleteAsync(TourLog log);
    Task<bool> ExistsAsync(int id);
}
