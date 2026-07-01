using Microsoft.EntityFrameworkCore;
using TourPlannerAPI.Data;
using TourPlannerAPI.Models;

namespace TourPlannerAPI.Repositories;

/// <summary>EF Core implementation of <see cref="ITourLogRepository"/>.</summary>
public class TourLogRepository : ITourLogRepository
{
    private readonly TourPlannerDbContext _context;

    public TourLogRepository(TourPlannerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TourLog>> GetByTourIdAsync(int tourId)
    {
        return await _context.TourLogs
            .Where(l => l.TourId == tourId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<TourLog?> GetByIdAsync(int id)
    {
        return await _context.TourLogs
            .Include(l => l.Tour)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<TourLog> AddAsync(TourLog log)
    {
        _context.TourLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task UpdateAsync(TourLog log)
    {
        _context.TourLogs.Update(log);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TourLog log)
    {
        _context.TourLogs.Remove(log);
        await _context.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(int id)
    {
        return _context.TourLogs.AnyAsync(l => l.Id == id);
    }
}
