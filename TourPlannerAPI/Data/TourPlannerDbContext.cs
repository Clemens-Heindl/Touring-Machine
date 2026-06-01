using Microsoft.EntityFrameworkCore;
using TourPlannerAPI.Models;

namespace TourPlannerAPI.Data
{
    public class TourPlannerDbContext : DbContext
    {
        public TourPlannerDbContext(DbContextOptions<TourPlannerDbContext> options) : base(options)
        {
        }

        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourLog> TourLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tour>()
                .HasMany(t => t.Logs)
                .WithOne(l => l.Tour)
                .HasForeignKey(l => l.TourId)
                .OnDelete(DeleteBehavior.Cascade); // If a tour is deleted, its logs are also deleted.
        }
    }
}
