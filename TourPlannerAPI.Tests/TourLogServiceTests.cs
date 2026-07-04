using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TourPlannerAPI.Dtos;
using TourPlannerAPI.Exceptions;
using TourPlannerAPI.Models;
using TourPlannerAPI.Repositories;
using TourPlannerAPI.Services;

namespace TourPlannerAPI.Tests;

[TestFixture]
public class TourLogServiceTests
{
    private Mock<ITourLogRepository> _logs = null!;
    private Mock<ITourRepository> _tours = null!;
    private TourLogService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _logs = new Mock<ITourLogRepository>();
        _tours = new Mock<ITourRepository>();
        _service = new TourLogService(_logs.Object, _tours.Object, NullLogger<TourLogService>.Instance);
    }

    private static SaveTourLogRequest ValidRequest() => new()
    {
        DateTime = new DateTime(2026, 4, 1),
        Difficulty = 3,
        TotalDistance = 10,
        TotalTime = TimeSpan.FromHours(1),
        Rating = 4
    };

    [Test]
    public async Task CreateAsync_StampsLogWithUser_WhenTourOwned()
    {
        _tours.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(TestData.Tour(1, userId: 5));
        TourLog? captured = null;
        _logs.Setup(r => r.AddAsync(It.IsAny<TourLog>()))
            .ReturnsAsync((TourLog l) => { l.Id = 9; captured = l; return l; });

        await _service.CreateAsync(1, ValidRequest(), userId: 5);

        Assert.That(captured!.UserId, Is.EqualTo(5));
        Assert.That(captured.TourId, Is.EqualTo(1));
    }

    [Test]
    public void CreateAsync_Throws_Forbidden_WhenTourNotOwned()
    {
        _tours.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(TestData.Tour(1, userId: 5));
        Assert.That(async () => await _service.CreateAsync(1, ValidRequest(), userId: 6),
            Throws.TypeOf<ForbiddenException>());
    }

    [Test]
    public void CreateAsync_Throws_NotFound_WhenTourMissing()
    {
        _tours.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Tour?)null);
        Assert.That(async () => await _service.CreateAsync(1, ValidRequest(), userId: 5),
            Throws.TypeOf<NotFoundException>());
    }

    [Test]
    public void CreateAsync_Throws_Validation_ForDifficultyOutOfRange()
    {
        _tours.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(TestData.Tour(1, userId: 5));
        var request = ValidRequest();
        request.Difficulty = 6;
        Assert.That(async () => await _service.CreateAsync(1, request, userId: 5),
            Throws.TypeOf<ValidationException>());
    }

    [Test]
    public void UpdateAsync_Throws_Forbidden_WhenLogNotOwned()
    {
        _logs.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new TourLog { Id = 3, UserId = 5, TourId = 1 });
        Assert.That(async () => await _service.UpdateAsync(3, ValidRequest(), userId: 6),
            Throws.TypeOf<ForbiddenException>());
    }

    [Test]
    public void DeleteAsync_Throws_NotFound_WhenLogMissing()
    {
        _logs.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((TourLog?)null);
        Assert.That(async () => await _service.DeleteAsync(3, userId: 5),
            Throws.TypeOf<NotFoundException>());
    }
}
