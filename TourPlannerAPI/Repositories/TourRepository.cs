using Microsoft.EntityFrameworkCore;
using TourPlannerAPI.Data;
using TourPlannerAPI.Models;

namespace TourPlannerAPI.Repositories;

/// <summary>EF Core implementation of <see cref="ITourRepository"/>.</summary>
public class TourRepository : ITourRepository
{
    private readonly TourPlannerDbContext _context;

    public TourRepository(TourPlannerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Tour>> GetAllByUserAsync(int userId)
    {
        return await _context.Tours
            .Include(t => t.Logs)
            .Where(t => t.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Tour?> GetByIdAsync(int id)
    {
        return await _context.Tours
            .Include(t => t.Logs)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tour> AddAsync(Tour tour)
    {
        _context.Tours.Add(tour);
        await _context.SaveChangesAsync();
        return tour;
    }

    public async Task UpdateAsync(Tour tour)
    {
        _context.Tours.Update(tour);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Tour tour)
    {
        _context.Tours.Remove(tour);
        await _context.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(int id)
    {
        return _context.Tours.AnyAsync(t => t.Id == id);
    }
}
