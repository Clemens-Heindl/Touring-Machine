using System.Text;
using System.Text.Json;
using TourPlannerAPI.Models;

namespace TourPlannerAPI.Services;

public class RouteService : IRouteService
{
    private static readonly Dictionary<string, string> ProfileMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "Car",  "driving-car"     },
            { "Bike", "cycling-regular" },
            { "Hike", "foot-hiking"     },
            { "Walk", "foot-walking"    }
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
            coordinates = new[] { fromCoords, toCoords },
            elevation = true
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

            var geometry = doc.RootElement
                .GetProperty("features")[0]
                .GetProperty("geometry")
                .GetProperty("coordinates");
            var elevation = ComputeElevation(geometry);

            _logger.LogInformation(
                "Route found: {DistanceKm} km, {Minutes} min, +{Ascent}m/-{Descent}m ({From} → {To})",
                distanceKm, estimatedMin, elevation.AscentM, elevation.DescentM, from, to);

            return new RouteResult
            {
                RouteGeoJson    = json,
                DistanceKm      = distanceKm,
                EstimatedMinutes = estimatedMin,
                AscentM         = elevation.AscentM,
                DescentM        = elevation.DescentM,
                MinElevationM   = elevation.MinElevationM,
                MaxElevationM   = elevation.MaxElevationM,
                ElevationProfile = elevation.ElevationProfile
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse ORS directions response.");
            return null;
        }
    }

    /// <summary>
    /// Derives ascent, descent, min/max elevation and a downsampled
    /// distance-vs-elevation profile from ORS 3D geometry ([lon, lat, ele]).
    /// Exposed internally so it can be unit-tested against sample geometry.
    /// </summary>
    internal static RouteResult ComputeElevation(JsonElement coordinates)
    {
        var result = new RouteResult();
        if (coordinates.ValueKind != JsonValueKind.Array || coordinates.GetArrayLength() == 0)
        {
            return result;
        }

        double ascent = 0, descent = 0;
        double min = double.MaxValue, max = double.MinValue;
        double cumulativeMeters = 0;
        double? prevLon = null, prevLat = null, prevEle = null;
        var raw = new List<ElevationPoint>();

        foreach (var coord in coordinates.EnumerateArray())
        {
            if (coord.GetArrayLength() < 3) continue;

            var lon = coord[0].GetDouble();
            var lat = coord[1].GetDouble();
            var ele = coord[2].GetDouble();

            if (prevLon is not null)
            {
                cumulativeMeters += HaversineMeters(prevLat!.Value, prevLon.Value, lat, lon);
                var delta = ele - prevEle!.Value;
                if (delta > 0) ascent += delta;
                else descent += -delta;
            }

            min = Math.Min(min, ele);
            max = Math.Max(max, ele);
            raw.Add(new ElevationPoint { DistanceKm = Math.Round(cumulativeMeters / 1000.0, 3), ElevationM = Math.Round(ele, 1) });

            prevLon = lon;
            prevLat = lat;
            prevEle = ele;
        }

        result.AscentM = Math.Round(ascent);
        result.DescentM = Math.Round(descent);
        result.MinElevationM = min == double.MaxValue ? 0 : Math.Round(min);
        result.MaxElevationM = max == double.MinValue ? 0 : Math.Round(max);
        result.ElevationProfile = Downsample(raw, 200);
        return result;
    }

    private static List<ElevationPoint> Downsample(List<ElevationPoint> points, int maxPoints)
    {
        if (points.Count <= maxPoints) return points;

        var step = (int)Math.Ceiling(points.Count / (double)maxPoints);
        var sampled = new List<ElevationPoint>();
        for (var i = 0; i < points.Count; i += step)
        {
            sampled.Add(points[i]);
        }
        if (sampled[^1] != points[^1]) sampled.Add(points[^1]);
        return sampled;
    }

    private static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadius = 6371000;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return earthRadius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
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
