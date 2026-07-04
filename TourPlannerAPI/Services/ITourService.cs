using TourPlannerAPI.Dtos;

namespace TourPlannerAPI.Services;

/// <summary>Business-layer operations for tours. All operations are scoped to the owning user.</summary>
public interface ITourService
{
    Task<IReadOnlyList<TourDto>> GetAllForUserAsync(int userId);
    Task<TourDto> GetByIdAsync(int id, int userId);
    Task<TourDto> CreateAsync(SaveTourRequest request, int userId);
    Task<TourDto> UpdateAsync(int id, SaveTourRequest request, int userId);
    Task DeleteAsync(int id, int userId);
    Task<IReadOnlyList<TourDto>> SearchAsync(int userId, string? query);
    Task<IReadOnlyList<TourDto>> ImportAsync(int userId, IEnumerable<TourImportDto> tours);
}
