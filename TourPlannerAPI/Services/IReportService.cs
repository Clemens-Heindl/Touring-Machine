namespace TourPlannerAPI.Services;

/// <summary>Generates PDF reports for tours and overall statistics.</summary>
public interface IReportService
{
    Task<byte[]> GenerateTourReportAsync(int tourId, int userId);
    Task<byte[]> GenerateSummaryReportAsync(int userId);
}
