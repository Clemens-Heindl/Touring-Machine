using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TourPlannerAPI.Data;

/// <summary>
/// Used by the EF Core command-line tools (migrations) so they build the
/// DbContext from configuration instead of running Program.cs (which would
/// trigger the startup auto-migration).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TourPlannerDbContext>
{
    public TourPlannerDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var options = new DbContextOptionsBuilder<TourPlannerDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new TourPlannerDbContext(options);
    }
}
