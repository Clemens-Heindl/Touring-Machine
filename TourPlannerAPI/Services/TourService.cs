using TourPlannerAPI.Dtos;
using TourPlannerAPI.Exceptions;
using TourPlannerAPI.Mapping;
using TourPlannerAPI.Models;
using TourPlannerAPI.Repositories;

namespace TourPlannerAPI.Services;

/// <summary>
/// Business logic for tours. Owns validation, ownership enforcement and
/// orchestration; talks only to the repository layer, never to EF Core directly.
/// Tours belong to a single user and are never shared.
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

    public async Task<IReadOnlyList<TourDto>> GetAllForUserAsync(int userId)
    {
        var tours = await _tours.GetAllByUserAsync(userId);
        return tours.Select(t => t.ToDto()).ToList();
    }

    public async Task<TourDto> GetByIdAsync(int id, int userId)
    {
        return (await GetOwnedTourAsync(id, userId)).ToDto();
    }

    public async Task<TourDto> CreateAsync(SaveTourRequest request, int userId)
    {
        Validate(request);
        var entity = request.ToEntity();
        entity.UserId = userId;
        var created = await _tours.AddAsync(entity);
        _logger.LogInformation("User {UserId} created tour {TourId} '{Name}'", userId, created.Id, created.Name);
        return created.ToDto();
    }

    public async Task<TourDto> UpdateAsync(int id, SaveTourRequest request, int userId)
    {
        Validate(request);
        var existing = await GetOwnedTourAsync(id, userId);

        request.ApplyTo(existing);
        await _tours.UpdateAsync(existing);
        _logger.LogInformation("User {UserId} updated tour {TourId}", userId, id);
        return existing.ToDto();
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var existing = await GetOwnedTourAsync(id, userId);
        await _tours.DeleteAsync(existing);
        _logger.LogInformation("User {UserId} deleted tour {TourId}", userId, id);
    }

    private async Task<Tour> GetOwnedTourAsync(int id, int userId)
    {
        var tour = await _tours.GetByIdAsync(id)
            ?? throw new NotFoundException("Tour", id);

        if (tour.UserId != userId)
            throw new ForbiddenException("You do not have access to this tour.");

        return tour;
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
