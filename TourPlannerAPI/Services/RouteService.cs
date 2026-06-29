using System.Text;
using System.Text.Json;
using TourPlannerAPI.Models;

namespace TourPlannerAPI.Services;

public class RouteService : IRouteService
{
    private static readonly Dictionary<string, string> ProfileMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "Bike",     "cycling-regular" },
            { "Hike",     "foot-hiking"     },
            { "Running",  "foot-running"    },
            { "Vacation", "driving-car"     }
        };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RouteService> _logger;

    public RouteService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<RouteService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<RouteResult?> GetRouteAsync(string from, string to, string transportType)
    {
        var apiKey = _configuration["OpenRouteService:ApiKey"];
        var baseUrl = _configuration["OpenRouteService:BaseUrl"]
                      ?? "https://api.openrouteservice.org";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("OpenRouteService:ApiKey is not configured.");
            return null;
        }

        if (!ProfileMap.TryGetValue(transportType, out var profile))
        {
            _logger.LogWarning(
                "Unknown transport type '{TransportType}', defaulting to driving-car.",
                transportType);
            profile = "driving-car";
        }

        var client = _httpClientFactory.CreateClient();

        var fromCoords = await GeocodeAsync(client, baseUrl, apiKey, from);
        if (fromCoords is null)
        {
            _logger.LogError("Could not geocode 'from' location: {From}", from);
            return null;
        }

        var toCoords = await GeocodeAsync(client, baseUrl, apiKey, to);
        if (toCoords is null)
        {
            _logger.LogError("Could not geocode 'to' location: {To}", to);
            return null;
        }

        var directionsUrl = $"{baseUrl}/v2/directions/{profile}/geojson";
        var body = JsonSerializer.Serialize(new
        {
            coordinates = new[] { fromCoords, toCoords }
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, directionsUrl);
        request.Headers.TryAddWithoutValidation("Authorization", apiKey);
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP error calling ORS directions endpoint.");
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "ORS directions returned {Status}: {Body}",
                response.StatusCode, err);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();

        try
        {
            using var doc = JsonDocument.Parse(json);
            var summary = doc.RootElement
                .GetProperty("features")[0]
                .GetProperty("properties")
                .GetProperty("summary");

            var distanceKm   = Math.Round(summary.GetProperty("distance").GetDouble() / 1000.0, 2);
            var estimatedMin = (int)(summary.GetProperty("duration").GetDouble() / 60.0);

            _logger.LogInformation(
                "Route found: {DistanceKm} km, {Minutes} min ({From} → {To})",
                distanceKm, estimatedMin, from, to);

            return new RouteResult
            {
                RouteGeoJson    = json,
                DistanceKm      = distanceKm,
                EstimatedMinutes = estimatedMin
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse ORS directions response.");
            return null;
        }
    }

    private async Task<double[]?> GeocodeAsync(
        HttpClient client, string baseUrl, string apiKey, string location)
    {
        var url = $"{baseUrl}/geocode/search" +
                  $"?text={Uri.EscapeDataString(location)}&size=1";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("Authorization", apiKey);

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP error geocoding '{Location}'.", location);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "ORS geocoding returned {Status} for '{Location}'.",
                response.StatusCode, location);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        try
        {
            using var doc = JsonDocument.Parse(json);
            var coords = doc.RootElement
                .GetProperty("features")[0]
                .GetProperty("geometry")
                .GetProperty("coordinates");

            return [coords[0].GetDouble(), coords[1].GetDouble()];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse geocoding response for '{Location}'.", location);
            return null;
        }
    }
}
