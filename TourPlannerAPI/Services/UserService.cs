using TourPlannerAPI.Dtos;
using TourPlannerAPI.Exceptions;
using TourPlannerAPI.Mapping;
using TourPlannerAPI.Models;
using TourPlannerAPI.Repositories;
using TourPlannerAPI.Utilities;

namespace TourPlannerAPI.Services;

/// <summary>Business logic for users and authentication.</summary>
public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository users, ILogger<UserService> logger)
    {
        _users = users;
        _logger = logger;
    }

    public async Task<UserDto> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim();

        if (await _users.ExistsByEmailAsync(email))
            throw new ConflictException("An account with this email already exists.");

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = request.PasswordHash
        };

        var created = await _users.AddAsync(user);
        _logger.LogInformation("Registered user {UserId} ({Email})", created.Id, created.Email);
        return created.ToDto();
    }

    public async Task<UserDto?> LoginAsync(string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(passwordHash))
            return null;

        var user = await _users.GetByEmailAsync(email.Trim());
        if (user is null || !PasswordHelper.VerifyPassword(passwordHash, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Email}", email);
            return null;
        }

        return user.ToDto();
    }

    public async Task<UserDto> GetByIdAsync(int id)
    {
        var user = await _users.GetByIdAsync(id)
            ?? throw new NotFoundException("User", id);
        return user.ToDto();
    }
}
