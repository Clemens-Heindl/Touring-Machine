using TourPlannerAPI.Models;

namespace TourPlannerAPI.Repositories;

/// <summary>
/// Data-access abstraction for tours. Business logic depends on this
/// interface, never on the EF Core <see cref="Data.TourPlannerDbContext"/>
/// directly, keeping the DAL swappable and testable.
/// </summary>
public interface ITourRepository
{
    Task<IReadOnlyList<Tour>> GetAllAsync();
    Task<IReadOnlyList<Tour>> GetAllByUserAsync(int userId);
    Task<Tour?> GetByIdAsync(int id);
    Task<Tour> AddAsync(Tour tour);
    Task UpdateAsync(Tour tour);
    Task DeleteAsync(Tour tour);
    Task<bool> ExistsAsync(int id);
}
