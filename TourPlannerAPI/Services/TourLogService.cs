using TourPlannerAPI.Dtos;
using TourPlannerAPI.Exceptions;
using TourPlannerAPI.Mapping;
using TourPlannerAPI.Repositories;

namespace TourPlannerAPI.Services;

/// <summary>Business logic for tour logs. Multiple logs belong to one tour.</summary>
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

    public async Task<IReadOnlyList<TourLogDto>> GetByTourAsync(int tourId)
    {
        await EnsureTourExists(tourId);
        var logs = await _logs.GetByTourIdAsync(tourId);
        return logs.Select(l => l.ToDto()).ToList();
    }

    public async Task<TourLogDto> CreateAsync(int tourId, SaveTourLogRequest request)
    {
        await EnsureTourExists(tourId);
        Validate(request);
        var created = await _logs.AddAsync(request.ToEntity(tourId));
        _logger.LogInformation("Created log {LogId} for tour {TourId}", created.Id, tourId);
        return created.ToDto();
    }

    public async Task<TourLogDto> UpdateAsync(int logId, SaveTourLogRequest request)
    {
        Validate(request);
        var existing = await _logs.GetByIdAsync(logId)
            ?? throw new NotFoundException("Tour log", logId);

        request.ApplyTo(existing);
        await _logs.UpdateAsync(existing);
        _logger.LogInformation("Updated log {LogId}", logId);
        return existing.ToDto();
    }

    public async Task DeleteAsync(int logId)
    {
        var existing = await _logs.GetByIdAsync(logId)
            ?? throw new NotFoundException("Tour log", logId);
        await _logs.DeleteAsync(existing);
        _logger.LogInformation("Deleted log {LogId}", logId);
    }

    private async Task EnsureTourExists(int tourId)
    {
        if (!await _tours.ExistsAsync(tourId))
            throw new NotFoundException("Tour", tourId);
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
