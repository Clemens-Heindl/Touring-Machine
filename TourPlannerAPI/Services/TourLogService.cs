using TourPlannerAPI.Dtos;
using TourPlannerAPI.Exceptions;
using TourPlannerAPI.Mapping;
using TourPlannerAPI.Models;
using TourPlannerAPI.Repositories;

namespace TourPlannerAPI.Services;

/// <summary>
/// Business logic for tour logs. Multiple logs belong to one tour; both the
/// parent tour and the log itself must belong to the requesting user.
/// </summary>
public class TourLogService : ITourLogService
{
    private readonly ITourLogRepository _logs;
    private readonly ITourRepository _tours;
    private readonly ILogger<TourLogService> _logger;

    public TourLogService(
        ITourLogRepository logs,
        ITourRepository tours,
        ILogger<TourLogService> logger)
    {
        _logs = logs;
        _tours = tours;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TourLogDto>> GetByTourAsync(int tourId, int userId)
    {
        await GetOwnedTourAsync(tourId, userId);
        var logs = await _logs.GetByTourIdAsync(tourId);
        return logs.Select(l => l.ToDto()).ToList();
    }

    public async Task<TourLogDto> CreateAsync(int tourId, SaveTourLogRequest request, int userId)
    {
        await GetOwnedTourAsync(tourId, userId);
        Validate(request);

        var entity = request.ToEntity(tourId);
        entity.UserId = userId;
        var created = await _logs.AddAsync(entity);
        _logger.LogInformation("User {UserId} created log {LogId} for tour {TourId}", userId, created.Id, tourId);
        return created.ToDto();
    }

    public async Task<TourLogDto> UpdateAsync(int logId, SaveTourLogRequest request, int userId)
    {
        Validate(request);
        var existing = await GetOwnedLogAsync(logId, userId);

        request.ApplyTo(existing);
        await _logs.UpdateAsync(existing);
        _logger.LogInformation("User {UserId} updated log {LogId}", userId, logId);
        return existing.ToDto();
    }

    public async Task DeleteAsync(int logId, int userId)
    {
        var existing = await GetOwnedLogAsync(logId, userId);
        await _logs.DeleteAsync(existing);
        _logger.LogInformation("User {UserId} deleted log {LogId}", userId, logId);
    }

    private async Task GetOwnedTourAsync(int tourId, int userId)
    {
        var tour = await _tours.GetByIdAsync(tourId)
            ?? throw new NotFoundException("Tour", tourId);

        if (tour.UserId != userId)
            throw new ForbiddenException("You do not have access to this tour.");
    }

    private async Task<TourLog> GetOwnedLogAsync(int logId, int userId)
    {
        var log = await _logs.GetByIdAsync(logId)
            ?? throw new NotFoundException("Tour log", logId);

        if (log.UserId != userId)
            throw new ForbiddenException("You do not have access to this tour log.");

        return log;
    }

    private static void Validate(SaveTourLogRequest request)
    {
        var errors = new List<string>();

        if (request.Difficulty is < 1 or > 5)
            errors.Add("Difficulty must be between 1 and 5.");
        if (request.Rating is < 1 or > 5)
            errors.Add("Rating must be between 1 and 5.");
        if (request.TotalDistance < 0)
            errors.Add("Total distance cannot be negative.");
        if (request.TotalTime < TimeSpan.Zero)
            errors.Add("Total time cannot be negative.");

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }
}
