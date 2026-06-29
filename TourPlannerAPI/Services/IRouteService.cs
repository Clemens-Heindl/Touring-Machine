using TourPlannerAPI.Models;

namespace TourPlannerAPI.Services;

public interface IRouteService
{
    Task<RouteResult?> GetRouteAsync(string from, string to, string transportType);
}
