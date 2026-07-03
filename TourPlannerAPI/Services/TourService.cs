using TourPlannerAPI.Dtos;
using TourPlannerAPI.Exceptions;
using TourPlannerAPI.Mapping;
using TourPlannerAPI.Models;
using TourPlannerAPI.Repositories;

namespace TourPlannerAPI.Services;

/// <summary>
/// Business logic for tours. Owns validation, ownership enforcement, computed
/// attributes and full-text search; talks only to the repository layer, never to
/// EF Core directly. Tours belong to a single user and are never shared.
/// </summary>
public class TourService : ITourService
{
    private readonly ITourRepository _tours;
    private readonly ITourAttributeCalculator _attributes;
    private readonly ILogger<TourService> _logger;

    public TourService(
        ITourRepository tours,
        ITourAttributeCalculator attributes,
        ILogger<TourService> logger)
    {
        _tours = tours;
        _attributes = attributes;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TourDto>> GetAllForUserAsync(int userId)
    {
        var tours = await _tours.GetAllByUserAsync(userId);
        return tours.Select(ToDtoWithComputed).ToList();
    }

    public async Task<TourDto> GetByIdAsync(int id, int userId)
    {
        return ToDtoWithComputed(await GetOwnedTourAsync(id, userId));
    }

    public async Task<TourDto> CreateAsync(SaveTourRequest request, int userId)
    {
        Validate(request);
        var entity = request.ToEntity();
        entity.UserId = userId;
        var created = await _tours.AddAsync(entity);
        _logger.LogInformation("User {UserId} created tour {TourId} '{Name}'", userId, created.Id, created.Name);
        return ToDtoWithComputed(created);
    }

    public async Task<TourDto> UpdateAsync(int id, SaveTourRequest request, int userId)
    {
        Validate(request);
        var existing = await GetOwnedTourAsync(id, userId);

        request.ApplyTo(existing);
        await _tours.UpdateAsync(existing);
        _logger.LogInformation("User {UserId} updated tour {TourId}", userId, id);
        return ToDtoWithComputed(existing);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var existing = await GetOwnedTourAsync(id, userId);
        await _tours.DeleteAsync(existing);
        _logger.LogInformation("User {UserId} deleted tour {TourId}", userId, id);
    }

    public async Task<IReadOnlyList<TourDto>> SearchAsync(int userId, string? query)
    {
        var tours = await _tours.GetAllByUserAsync(userId);
        var dtos = tours.Select(ToDtoWithComputed).ToList();

        var tokens = (query ?? string.Empty)
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (tokens.Length == 0)
            return dtos;

        // Full-text match: every token must appear somewhere in the tour's
        // haystack, which includes the computed attributes.
        var results = dtos
            .Where(dto => tokens.All(token => BuildHaystack(dto).Contains(token)))
            .ToList();

        _logger.LogInformation("User {UserId} searched '{Query}' -> {Count} result(s)", userId, query, results.Count);
        return results;
    }

    public async Task<IReadOnlyList<TourDto>> ImportAsync(int userId, IEnumerable<TourImportDto> tours)
    {
        var created = new List<TourDto>();

        foreach (var import in tours)
        {
            if (string.IsNullOrWhiteSpace(import.Name))
                continue; // skip malformed entries rather than failing the whole import

            var entity = new Tour
            {
                Name = import.Name.Trim(),
                Description = import.Description,
                From = import.From?.Trim() ?? string.Empty,
                To = import.To?.Trim() ?? string.Empty,
                TransportType = import.TransportType?.Trim() ?? string.Empty,
                Distance = import.Distance,
                EstimatedTime = import.EstimatedTime,
                RouteInformation = import.RouteInformation,
                ImageFileName = import.ImageFileName,
                UserId = userId,
                Logs = import.Logs.Select(l => new TourLog
                {
                    DateTime = l.DateTime,
                    Comment = l.Comment,
                    Difficulty = l.Difficulty,
                    TotalDistance = l.TotalDistance,
                    TotalTime = l.TotalTime,
                    Rating = l.Rating,
                    UserId = userId
                }).ToList()
            };

            var saved = await _tours.AddAsync(entity);
            created.Add(ToDtoWithComputed(saved));
        }

        _logger.LogInformation("User {UserId} imported {Count} tour(s)", userId, created.Count);
        return created;
    }

    /// <summary>Concatenates every searchable field of a tour, computed values included.</summary>
    private static string BuildHaystack(TourDto dto)
    {
        var logText = string.Join(' ', dto.Logs.Select(l =>
            $"{l.Comment} {l.Difficulty} {l.Rating} {l.TotalDistance} {l.TotalTime}"));

        return string.Join(' ', new[]
        {
            dto.Name, dto.Description, dto.From, dto.To, dto.TransportType,
            dto.Distance.ToString(), dto.EstimatedTime.ToString(),
            dto.Popularity, dto.ChildFriendliness, logText
        }).ToLowerInvariant();
    }

    private TourDto ToDtoWithComputed(Tour tour)
    {
        var dto = tour.ToDto();
        dto.Popularity = _attributes.GetPopularity(tour);
        dto.ChildFriendliness = _attributes.GetChildFriendliness(tour);
        return dto;
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
