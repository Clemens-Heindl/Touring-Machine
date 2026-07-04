using System.Text.Json;
using TourPlannerAPI.Services;

namespace TourPlannerAPI.Tests;

[TestFixture]
public class RouteServiceElevationTests
{
    private static JsonElement Coordinates(string json)
        => JsonDocument.Parse(json).RootElement;

    [Test]
    public void ComputeElevation_SumsAscentAndDescent()
    {
        // [lon, lat, elevation]: 100 -> 150 (+50) -> 120 (-30)
        var coords = Coordinates("[[0,0,100],[0,0.001,150],[0,0.002,120]]");

        var result = RouteService.ComputeElevation(coords);

        Assert.That(result.AscentM, Is.EqualTo(50));
        Assert.That(result.DescentM, Is.EqualTo(30));
    }

    [Test]
    public void ComputeElevation_ReportsMinAndMax()
    {
        var coords = Coordinates("[[0,0,100],[0,0.001,150],[0,0.002,120]]");

        var result = RouteService.ComputeElevation(coords);

        Assert.That(result.MinElevationM, Is.EqualTo(100));
        Assert.That(result.MaxElevationM, Is.EqualTo(150));
    }

    [Test]
    public void ComputeElevation_BuildsProfileWithCumulativeDistance()
    {
        var coords = Coordinates("[[0,0,100],[0,0.001,150],[0,0.002,120]]");

        var result = RouteService.ComputeElevation(coords);

        Assert.That(result.ElevationProfile, Has.Count.EqualTo(3));
        Assert.That(result.ElevationProfile[0].DistanceKm, Is.EqualTo(0));
        Assert.That(result.ElevationProfile[2].DistanceKm, Is.GreaterThan(0));
    }

    [Test]
    public void ComputeElevation_EmptyGeometry_ReturnsZeroes()
    {
        var coords = Coordinates("[]");

        var result = RouteService.ComputeElevation(coords);

        Assert.That(result.AscentM, Is.EqualTo(0));
        Assert.That(result.DescentM, Is.EqualTo(0));
        Assert.That(result.ElevationProfile, Is.Empty);
    }
}
