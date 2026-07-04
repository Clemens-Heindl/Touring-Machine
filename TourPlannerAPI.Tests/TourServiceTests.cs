using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TourPlannerAPI.Dtos;
using TourPlannerAPI.Exceptions;
using TourPlannerAPI.Models;
using TourPlannerAPI.Repositories;
using TourPlannerAPI.Services;

namespace TourPlannerAPI.Tests;

[TestFixture]
public class TourServiceTests
{
    private Mock<ITourRepository> _repo = null!;
    private TourService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<ITourRepository>();
        _service = new TourService(_repo.Object, TestData.Calculator(), NullLogger<TourService>.Instance);
    }

    private static SaveTourRequest ValidRequest() => new()
    {
        Name = "New Tour",
        From = "A",
        To = "B",
        TransportType = "Bike",
        Distance = 12,
        EstimatedTime = TimeSpan.FromHours(1)
    };

    [Test]
    public async Task GetByIdAsync_ReturnsDto_WhenOwnedByUser()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(TestData.Tour(1, userId: 7));
        var dto = await _service.GetByIdAsync(1, userId: 7);
        Assert.That(dto.Id, Is.EqualTo(1));
    }

    [Test]
    public void GetByIdAsync_Throws_NotFound_WhenMissing()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Tour?)null);
        Assert.That(async () => await _service.GetByIdAsync(99, userId: 7),
            Throws.TypeOf<NotFoundException>());
    }

    [Test]
    public void GetByIdAsync_Throws_Forbidden_ForOtherUsersTour()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(TestData.Tour(1, userId: 7));
        Assert.That(async () => await _service.GetByIdAsync(1, userId: 8),
            Throws.TypeOf<ForbiddenException>());
    }

    [Test]
    public async Task CreateAsync_AssignsTourToCurrentUser()
    {
        Tour? captured = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<Tour>()))
            .ReturnsAsync((Tour t) => { t.Id = 5; captured = t; return t; });

        var dto = await _service.CreateAsync(ValidRequest(), userId: 42);

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.UserId, Is.EqualTo(42));
        Assert.That(dto.UserId, Is.EqualTo(42));
    }

    [Test]
    public void CreateAsync_Throws_ValidationException_ForEmptyName()
    {
        var request = ValidRequest();
        request.Name = "";
        Assert.That(async () => await _service.CreateAsync(request, userId: 1),
            Throws.TypeOf<ValidationException>());
    }

    [Test]
    public void CreateAsync_Throws_ValidationException_ForNegativeDistance()
    {
        var request = ValidRequest();
        request.Distance = -1;
        Assert.That(async () => await _service.CreateAsync(request, userId: 1),
            Throws.TypeOf<ValidationException>());
    }

    [Test]
    public void UpdateAsync_Throws_Forbidden_ForOtherUsersTour()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(TestData.Tour(1, userId: 7));
        Assert.That(async () => await _service.UpdateAsync(1, ValidRequest(), userId: 8),
            Throws.TypeOf<ForbiddenException>());
    }

    [Test]
    public async Task SearchAsync_MatchesByName()
    {
        var tours = new List<Tour> { TestData.Tour(1, 1), Named(2, "Alpine Ridge") };
        _repo.Setup(r => r.GetAllByUserAsync(1)).ReturnsAsync(tours);

        var results = await _service.SearchAsync(1, "alpine");

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].Id, Is.EqualTo(2));
    }

    [Test]
    public async Task SearchAsync_MatchesByComputedPopularity()
    {
        var popular = TestData.Tour(3, 1, TestData.Log(), TestData.Log(), TestData.Log());
        _repo.Setup(r => r.GetAllByUserAsync(1))
            .ReturnsAsync(new List<Tour> { TestData.Tour(1, 1), popular });

        var results = await _service.SearchAsync(1, "popular");

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].Id, Is.EqualTo(3));
    }

    [Test]
    public async Task SearchAsync_EmptyQuery_ReturnsAll()
    {
        _repo.Setup(r => r.GetAllByUserAsync(1))
            .ReturnsAsync(new List<Tour> { TestData.Tour(1, 1), TestData.Tour(2, 1) });

        var results = await _service.SearchAsync(1, "");

        Assert.That(results, Has.Count.EqualTo(2));
    }

    private static Tour Named(int id, string name)
    {
        var tour = TestData.Tour(id, 1);
        tour.Name = name;
        return tour;
    }
}
