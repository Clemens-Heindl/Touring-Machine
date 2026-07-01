using Microsoft.EntityFrameworkCore;
using TourPlannerAPI.Data;
using TourPlannerAPI.Models;

namespace TourPlannerAPI.Repositories;

/// <summary>EF Core implementation of <see cref="IUserRepository"/>.</summary>
public class UserRepository : IUserRepository
{
    private readonly TourPlannerDbContext _context;

    public UserRepository(TourPlannerDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public Task<bool> ExistsByEmailAsync(string email)
    {
        return _context.Users.AnyAsync(u => u.Email == email);
    }
}
