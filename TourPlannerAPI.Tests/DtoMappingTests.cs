using TourPlannerAPI.Dtos;
using TourPlannerAPI.Mapping;

namespace TourPlannerAPI.Tests;

[TestFixture]
public class DtoMappingTests
{
    [Test]
    public void AsUtc_TreatsUnspecifiedAsUtc_WithoutShifting()
    {
        var value = new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Unspecified);
        var result = DtoMappingExtensions.AsUtc(value);

        Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
        Assert.That(result.Hour, Is.EqualTo(9));
    }

    [Test]
    public void AsUtc_LeavesUtcUnchanged()
    {
        var value = new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Utc);
        Assert.That(DtoMappingExtensions.AsUtc(value), Is.EqualTo(value));
    }

    [Test]
    public void ToEntity_NormalisesLogDateToUtc()
    {
        var request = new SaveTourLogRequest
        {
            DateTime = new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Unspecified),
            Difficulty = 3,
            Rating = 4,
            TotalDistance = 10,
            TotalTime = TimeSpan.FromHours(1)
        };

        var entity = request.ToEntity(tourId: 1);

        Assert.That(entity.DateTime.Kind, Is.EqualTo(DateTimeKind.Utc));
    }
}
