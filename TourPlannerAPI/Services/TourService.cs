using TourPlannerAPI.Dtos;
using TourPlannerAPI.Exceptions;
using TourPlannerAPI.Mapping;
using TourPlannerAPI.Repositories;

namespace TourPlannerAPI.Services;

/// <summary>
/// Business logic for tours. Owns validation and orchestration; talks only to
/// the repository layer, never to EF Core directly.
/// </summary>
public class TourService : ITourService
{
    private readonly ITourRepository _tours;
    private readonly ILogger<TourService> _logger;

    public TourService(ITourRepository tours, ILogger<TourService> logger)
    {
        _tours = tours;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TourDto>> GetAllAsync()
    {
        var tours = await _tours.GetAllAsync();
        return tours.Select(t => t.ToDto()).ToList();
    }

    public async Task<TourDto> GetByIdAsync(int id)
    {
        var tour = await _tours.GetByIdAsync(id)
            ?? throw new NotFoundException("Tour", id);
        return tour.ToDto();
    }

    public async Task<TourDto> CreateAsync(SaveTourRequest request)
    {
        Validate(request);
        var created = await _tours.AddAsync(request.ToEntity());
        _logger.LogInformation("Created tour {TourId} '{Name}'", created.Id, created.Name);
        return created.ToDto();
    }

    public async Task<TourDto> UpdateAsync(int id, SaveTourRequest request)
    {
        Validate(request);
        var existing = await _tours.GetByIdAsync(id)
            ?? throw new NotFoundException("Tour", id);

        request.ApplyTo(existing);
        await _tours.UpdateAsync(existing);
        _logger.LogInformation("Updated tour {TourId}", id);
        return existing.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var existing = await _tours.GetByIdAsync(id)
            ?? throw new NotFoundException("Tour", id);
        await _tours.DeleteAsync(existing);
        _logger.LogInformation("Deleted tour {TourId}", id);
    }

    private static void Validate(SaveTourRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("Name is required.");
        if (string.IsNullOrWhiteSpace(request.From))
            errors.Add("Start location (from) is required.");
        if (string.IsNullOrWhiteSpace(request.To))
            errors.Add("Destination (to) is required.");
        if (request.Distance < 0)
            errors.Add("Distance cannot be negative.");
        if (request.EstimatedTime < TimeSpan.Zero)
            errors.Add("Estimated time cannot be negative.");

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }
}
