using TourPlannerAPI.Services;

namespace TourPlannerAPI.Tests;

[TestFixture]
public class TourAttributeCalculatorTests
{
    private ITourAttributeCalculator _calc = null!;

    [SetUp]
    public void SetUp() => _calc = TestData.Calculator();

    [Test]
    public void Popularity_NoLogs_IsNew()
    {
        var tour = TestData.Tour();
        Assert.That(_calc.GetPopularity(tour), Is.EqualTo(TourAttributeCalculator.PopularityNew));
    }

    [Test]
    public void Popularity_OneLog_IsKnown()
    {
        var tour = TestData.Tour(logs: TestData.Log());
        Assert.That(_calc.GetPopularity(tour), Is.EqualTo(TourAttributeCalculator.PopularityKnown));
    }

    [Test]
    public void Popularity_ThreeLogs_IsPopular()
    {
        var tour = TestData.Tour(logs: new[] { TestData.Log(), TestData.Log(), TestData.Log() });
        Assert.That(_calc.GetPopularity(tour), Is.EqualTo(TourAttributeCalculator.PopularityPopular));
    }

    [Test]
    public void ChildFriendliness_NoLogs_ShortEasyTour_IsChildFriendly()
    {
        var tour = TestData.Tour();
        tour.Distance = 5;
        tour.EstimatedTime = TimeSpan.FromHours(1);
        Assert.That(_calc.GetChildFriendliness(tour), Is.EqualTo(TourAttributeCalculator.ChildFriendly));
    }

    [Test]
    public void ChildFriendliness_NoLogs_LongHardTour_IsChallenging()
    {
        var tour = TestData.Tour();
        tour.Distance = 80;
        tour.EstimatedTime = TimeSpan.FromHours(9);
        Assert.That(_calc.GetChildFriendliness(tour), Is.EqualTo(TourAttributeCalculator.Challenging));
    }

    [Test]
    public void ChildFriendliness_EasyLogs_IsChildFriendly()
    {
        var tour = TestData.Tour(logs: new[] { TestData.Log(difficulty: 1, distance: 5, hours: 1) });
        Assert.That(_calc.GetChildFriendliness(tour), Is.EqualTo(TourAttributeCalculator.ChildFriendly));
    }

    [Test]
    public void ChildFriendliness_ModerateLogs_IsModerate()
    {
        var tour = TestData.Tour(logs: new[] { TestData.Log(difficulty: 3, distance: 20, hours: 5) });
        Assert.That(_calc.GetChildFriendliness(tour), Is.EqualTo(TourAttributeCalculator.Moderate));
    }

    [Test]
    public void ChildFriendliness_HardLogs_IsChallenging()
    {
        var tour = TestData.Tour(logs: new[] { TestData.Log(difficulty: 5, distance: 40, hours: 8) });
        Assert.That(_calc.GetChildFriendliness(tour), Is.EqualTo(TourAttributeCalculator.Challenging));
    }
}
