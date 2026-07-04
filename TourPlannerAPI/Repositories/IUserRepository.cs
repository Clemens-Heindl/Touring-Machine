using TourPlannerAPI.Models;

namespace TourPlannerAPI.Repositories;

/// <summary>Data-access abstraction for users.</summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> AddAsync(User user);
    Task<bool> ExistsByEmailAsync(string email);
}
