using TourPlannerAPI.Dtos;
using TourPlannerAPI.Models;

namespace TourPlannerAPI.Mapping;

/// <summary>Manual entity &lt;-&gt; DTO mapping so entities never cross the API boundary.</summary>
public static class DtoMappingExtensions
{
    public static TourDto ToDto(this Tour tour) => new()
    {
        Id = tour.Id,
        UserId = tour.UserId,
        Name = tour.Name,
        Description = tour.Description,
        From = tour.From,
        To = tour.To,
        TransportType = tour.TransportType,
        Distance = tour.Distance,
        EstimatedTime = tour.EstimatedTime,
        RouteInformation = tour.RouteInformation,
        ImageFileName = tour.ImageFileName,
        Logs = tour.Logs.Select(l => l.ToDto()).ToList()
    };

    public static Tour ToEntity(this SaveTourRequest request) => new()
    {
        Name = request.Name.Trim(),
        Description = request.Description,
        From = request.From.Trim(),
        To = request.To.Trim(),
        TransportType = request.TransportType.Trim(),
        Distance = request.Distance,
        EstimatedTime = request.EstimatedTime,
        RouteInformation = request.RouteInformation,
        ImageFileName = request.ImageFileName,
        UserId = request.UserId
    };

    public static void ApplyTo(this SaveTourRequest request, Tour tour)
    {
        tour.Name = request.Name.Trim();
        tour.Description = request.Description;
        tour.From = request.From.Trim();
        tour.To = request.To.Trim();
        tour.TransportType = request.TransportType.Trim();
        tour.Distance = request.Distance;
        tour.EstimatedTime = request.EstimatedTime;
        tour.RouteInformation = request.RouteInformation;
        tour.ImageFileName = request.ImageFileName;
    }

    public static TourLogDto ToDto(this TourLog log) => new()
    {
        Id = log.Id,
        DateTime = log.DateTime,
        Comment = log.Comment,
        Difficulty = log.Difficulty,
        TotalDistance = log.TotalDistance,
        TotalTime = log.TotalTime,
        Rating = log.Rating,
        TourId = log.TourId
    };

    public static TourLog ToEntity(this SaveTourLogRequest request, int tourId) => new()
    {
        DateTime = request.DateTime,
        Comment = request.Comment,
        Difficulty = request.Difficulty,
        TotalDistance = request.TotalDistance,
        TotalTime = request.TotalTime,
        Rating = request.Rating,
        TourId = tourId
    };

    public static void ApplyTo(this SaveTourLogRequest request, TourLog log)
    {
        log.DateTime = request.DateTime;
        log.Comment = request.Comment;
        log.Difficulty = request.Difficulty;
        log.TotalDistance = request.TotalDistance;
        log.TotalTime = request.TotalTime;
        log.Rating = request.Rating;
    }

    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email
    };
}
