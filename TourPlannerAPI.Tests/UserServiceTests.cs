using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TourPlannerAPI.Dtos;
using TourPlannerAPI.Exceptions;
using TourPlannerAPI.Models;
using TourPlannerAPI.Repositories;
using TourPlannerAPI.Services;
using TourPlannerAPI.Utilities;

namespace TourPlannerAPI.Tests;

[TestFixture]
public class UserServiceTests
{
    private Mock<IUserRepository> _users = null!;
    private UserService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _users = new Mock<IUserRepository>();
        _service = new UserService(_users.Object, NullLogger<UserService>.Instance);
    }

    [Test]
    public async Task RegisterAsync_HashesPassword_NeverStoresRaw()
    {
        User? captured = null;
        _users.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
        _users.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.Id = 1; captured = u; return u; });

        await _service.RegisterAsync(new RegisterRequest
        {
            Name = "Ada",
            Email = "ada@example.com",
            Password = "Secret123"
        });

        Assert.That(captured!.PasswordHash, Is.Not.EqualTo("Secret123"));
        Assert.That(PasswordHelper.VerifyPassword("Secret123", captured.PasswordHash), Is.True);
    }

    [Test]
    public void RegisterAsync_Throws_Conflict_ForDuplicateEmail()
    {
        _users.Setup(r => r.ExistsByEmailAsync("ada@example.com")).ReturnsAsync(true);
        Assert.That(async () => await _service.RegisterAsync(new RegisterRequest
        {
            Name = "Ada",
            Email = "ada@example.com",
            Password = "Secret123"
        }), Throws.TypeOf<ConflictException>());
    }

    [Test]
    public async Task LoginAsync_ReturnsUser_ForCorrectPassword()
    {
        _users.Setup(r => r.GetByEmailAsync("ada@example.com")).ReturnsAsync(new User
        {
            Id = 1,
            Email = "ada@example.com",
            Name = "Ada",
            PasswordHash = PasswordHelper.HashPassword("Secret123")
        });

        var result = await _service.LoginAsync("ada@example.com", "Secret123");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task LoginAsync_ReturnsNull_ForWrongPassword()
    {
        _users.Setup(r => r.GetByEmailAsync("ada@example.com")).ReturnsAsync(new User
        {
            Email = "ada@example.com",
            PasswordHash = PasswordHelper.HashPassword("Secret123")
        });

        var result = await _service.LoginAsync("ada@example.com", "WrongPassword");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task LoginAsync_ReturnsNull_ForUnknownEmail()
    {
        _users.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        var result = await _service.LoginAsync("nobody@example.com", "Secret123");
        Assert.That(result, Is.Null);
    }
}
