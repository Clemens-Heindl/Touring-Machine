using TourPlannerAPI.Dtos;

namespace TourPlannerAPI.Services;

/// <summary>Business-layer operations for tours.</summary>
public interface ITourService
{
    Task<IReadOnlyList<TourDto>> GetAllAsync();
    Task<TourDto> GetByIdAsync(int id);
    Task<TourDto> CreateAsync(SaveTourRequest request);
    Task<TourDto> UpdateAsync(int id, SaveTourRequest request);
    Task DeleteAsync(int id);
}
