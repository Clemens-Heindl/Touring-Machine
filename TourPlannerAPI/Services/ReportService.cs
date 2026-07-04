using System.Globalization;
using System.Text;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TourPlannerAPI.Dtos;
using TourPlannerAPI.Models;

namespace TourPlannerAPI.Services;

/// <summary>
/// Generates PDF reports with QuestPDF. Reuses the business layer for data and
/// embeds the elevation profile (and a route-shape sketch) as inline SVG.
/// </summary>
public class ReportService : IReportService
{
    private const string Green = "#1baf7a";
    private const string GreenFill = "#d7efe4";
    private const string Blue = "#2a78d6";
    private const string Ink = "#0b0b0b";
    private const string Muted = "#898781";

    private readonly ITourService _tours;
    private readonly IStatisticsService _statistics;
    private readonly ILogger<ReportService> _logger;

    public ReportService(ITourService tours, IStatisticsService statistics, ILogger<ReportService> logger)
    {
        _tours = tours;
        _statistics = statistics;
        _logger = logger;
    }

    public async Task<byte[]> GenerateTourReportAsync(int tourId, int userId)
    {
        var tour = await _tours.GetByIdAsync(tourId, userId);
        var (elevation, lonLat) = ParseGeometry(tour.RouteInformation);

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(t => t.FontSize(10).FontColor(Ink));

                page.Header().Column(col =>
                {
                    col.Item().Text("Tour Report").FontSize(20).Bold();
                    col.Item().Text(tour.Name).FontSize(14).FontColor(Blue);
                    col.Item().Text($"Generated {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(8).FontColor(Muted);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(14);

                    if (!string.IsNullOrWhiteSpace(tour.Description))
                        col.Item().Text(tour.Description).FontColor(Muted);

                    col.Item().Element(c => AttributeGrid(c, tour, elevation));

                    if (elevation.ElevationProfile.Count >= 2)
                    {
                        col.Item().Text("Elevation profile").Bold();
                        col.Item().Svg(BuildElevationSvg(elevation.ElevationProfile));
                    }

                    if (lonLat.Count >= 2)
                    {
                        col.Item().Text("Route shape").Bold();
                        col.Item().Svg(BuildRouteSvg(lonLat));
                    }

                    col.Item().Text($"Tour logs ({tour.Logs.Count})").Bold();
                    col.Item().Element(c => LogsTable(c, tour.Logs));
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Page ");
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });
        });

        _logger.LogInformation("Generated tour report for tour {TourId} (user {UserId})", tourId, userId);
        return pdf.GeneratePdf();
    }

    public async Task<byte[]> GenerateSummaryReportAsync(int userId)
    {
        var stats = await _statistics.GetForUserAsync(userId);
        var tours = await _tours.GetAllForUserAsync(userId);

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(t => t.FontSize(10).FontColor(Ink));

                page.Header().Column(col =>
                {
                    col.Item().Text("Tour Summary Report").FontSize(20).Bold();
                    col.Item().Text($"Generated {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(8).FontColor(Muted);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(14);

                    col.Item().Row(row =>
                    {
                        row.Spacing(10);
                        Kpi(row, "Tours", stats.TourCount.ToString());
                        Kpi(row, "Logs", stats.LogCount.ToString());
                        Kpi(row, "Distance", $"{stats.TotalLoggedDistanceKm} km");
                        Kpi(row, "Time", $"{stats.TotalLoggedTimeHours} h");
                        Kpi(row, "Avg rating", $"{stats.AverageRating}/5");
                        Kpi(row, "Avg diff.", $"{stats.AverageDifficulty}/5");
                    });

                    col.Item().Text("Tours by transport type").Bold();
                    foreach (var t in stats.ByTransportType)
                        col.Item().Text($"{t.TransportType}: {t.TourCount}");

                    col.Item().Text("All tours").Bold();
                    col.Item().Element(c => TourTable(c, tours));
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Page ");
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });
        });

        _logger.LogInformation("Generated summary report for user {UserId}", userId);
        return pdf.GeneratePdf();
    }

    private static void Kpi(RowDescriptor row, string label, string value)
    {
        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
        {
            c.Item().Text(label).FontSize(8).FontColor(Muted);
            c.Item().Text(value).FontSize(13).Bold();
        });
    }

    private static void AttributeGrid(IContainer container, TourDto tour, RouteResult elevation)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });

            void Row(string label, string value)
            {
                table.Cell().PaddingVertical(2).Text(t => { t.Span($"{label}: ").FontColor(Muted); t.Span(value); });
            }

            Row("From", tour.From);
            Row("To", tour.To);
            Row("Transport", tour.TransportType);
            Row("Distance", $"{tour.Distance} km");
            Row("Estimated time", tour.EstimatedTime.ToString());
            Row("Popularity", tour.Popularity);
            Row("Child friendliness", tour.ChildFriendliness);
            if (elevation.ElevationProfile.Count >= 2)
            {
                Row("Ascent", $"{elevation.AscentM} m");
                Row("Descent", $"{elevation.DescentM} m");
            }
        });
    }

    private static void LogsTable(IContainer container, IReadOnlyList<TourLogDto> logs)
    {
        if (logs.Count == 0)
        {
            container.Text("No logs recorded.").FontColor(Muted);
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(90); c.ConstantColumn(50); c.ConstantColumn(60);
                c.ConstantColumn(55); c.ConstantColumn(45); c.RelativeColumn();
            });

            void Header(string text) =>
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(text).Bold().FontSize(9);

            Header("Date"); Header("Diff."); Header("Distance"); Header("Time"); Header("Rating"); Header("Comment");

            foreach (var log in logs)
            {
                table.Cell().Padding(4).Text(log.DateTime.ToString("yyyy-MM-dd")).FontSize(9);
                table.Cell().Padding(4).Text(log.Difficulty.ToString()).FontSize(9);
                table.Cell().Padding(4).Text($"{log.TotalDistance} km").FontSize(9);
                table.Cell().Padding(4).Text(log.TotalTime.ToString()).FontSize(9);
                table.Cell().Padding(4).Text(log.Rating.ToString()).FontSize(9);
                table.Cell().Padding(4).Text(log.Comment ?? string.Empty).FontSize(9);
            }
        });
    }

    private static void TourTable(IContainer container, IReadOnlyList<TourDto> tours)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(2); c.ConstantColumn(60); c.ConstantColumn(60);
                c.ConstantColumn(45); c.RelativeColumn();
            });

            void Header(string text) =>
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(text).Bold().FontSize(9);

            Header("Name"); Header("Transport"); Header("Distance"); Header("Logs"); Header("Child-friendly");

            foreach (var t in tours)
            {
                table.Cell().Padding(4).Text(t.Name).FontSize(9);
                table.Cell().Padding(4).Text(t.TransportType).FontSize(9);
                table.Cell().Padding(4).Text($"{t.Distance} km").FontSize(9);
                table.Cell().Padding(4).Text(t.Logs.Count.ToString()).FontSize(9);
                table.Cell().Padding(4).Text(t.ChildFriendliness).FontSize(9);
            }
        });
    }

    // --- geometry parsing + SVG builders ---

    private static (RouteResult Elevation, List<double[]> LonLat) ParseGeometry(string? geoJson)
    {
        var lonLat = new List<double[]>();
        if (string.IsNullOrWhiteSpace(geoJson))
            return (new RouteResult(), lonLat);

        try
        {
            using var doc = JsonDocument.Parse(geoJson);
            var coordinates = doc.RootElement
                .GetProperty("features")[0]
                .GetProperty("geometry")
                .GetProperty("coordinates");

            var elevation = RouteService.ComputeElevation(coordinates);

            foreach (var coord in coordinates.EnumerateArray())
            {
                if (coord.GetArrayLength() >= 2)
                    lonLat.Add(new[] { coord[0].GetDouble(), coord[1].GetDouble() });
            }

            return (elevation, lonLat);
        }
        catch
        {
            return (new RouteResult(), lonLat);
        }
    }

    private static string F(double value) => value.ToString("0.#", CultureInfo.InvariantCulture);

    private static string BuildElevationSvg(IReadOnlyList<ElevationPoint> profile)
    {
        const int w = 500, h = 160, padL = 42, padR = 10, padT = 12, padB = 24;
        double innerW = w - padL - padR, innerH = h - padT - padB, baseY = padT + innerH;
        double min = profile.Min(p => p.ElevationM), max = profile.Max(p => p.ElevationM);
        var range = max - min <= 0 ? 1 : max - min;
        var totalDist = profile[^1].DistanceKm <= 0 ? 1 : profile[^1].DistanceKm;

        var pts = profile
            .Select(p => (x: padL + p.DistanceKm / totalDist * innerW,
                          y: padT + innerH - (p.ElevationM - min) / range * innerH))
            .ToList();

        var line = string.Join(" ", pts.Select((p, i) => $"{(i == 0 ? "M" : "L")} {F(p.x)} {F(p.y)}"));
        var area = $"M {F(pts[0].x)} {F(baseY)} " +
                   string.Join(" ", pts.Select(p => $"L {F(p.x)} {F(p.y)}")) +
                   $" L {F(pts[^1].x)} {F(baseY)} Z";

        var sb = new StringBuilder();
        sb.Append($"<svg xmlns='http://www.w3.org/2000/svg' width='{w}' height='{h}' viewBox='0 0 {w} {h}'>");
        for (var i = 0; i <= 3; i++)
        {
            var value = min + range / 3 * i;
            var y = baseY - (value - min) / range * innerH;
            sb.Append($"<line x1='{padL}' y1='{F(y)}' x2='{w - padR}' y2='{F(y)}' stroke='#e1e0d9' stroke-width='1'/>");
            sb.Append($"<text x='{padL - 4}' y='{F(y + 4)}' text-anchor='end' font-size='10' fill='{Muted}'>{Math.Round(value)}</text>");
        }
        sb.Append($"<path d='{area}' fill='{GreenFill}'/>");
        sb.Append($"<path d='{line}' fill='none' stroke='{Green}' stroke-width='2'/>");
        sb.Append("</svg>");
        return sb.ToString();
    }

    private static string BuildRouteSvg(List<double[]> lonLat)
    {
        const int w = 500, h = 200, pad = 12;
        double minLon = lonLat.Min(c => c[0]), maxLon = lonLat.Max(c => c[0]);
        double minLat = lonLat.Min(c => c[1]), maxLat = lonLat.Max(c => c[1]);
        var spanLon = maxLon - minLon <= 0 ? 1 : maxLon - minLon;
        var spanLat = maxLat - minLat <= 0 ? 1 : maxLat - minLat;
        var scale = Math.Min((w - 2 * pad) / spanLon, (h - 2 * pad) / spanLat);
        var offsetX = (w - spanLon * scale) / 2;
        var offsetY = (h - spanLat * scale) / 2;

        var pts = lonLat.Select(c =>
        {
            var x = offsetX + (c[0] - minLon) * scale;
            var y = h - (offsetY + (c[1] - minLat) * scale); // flip so north is up
            return $"{F(x)},{F(y)}";
        });

        return $"<svg xmlns='http://www.w3.org/2000/svg' width='{w}' height='{h}' viewBox='0 0 {w} {h}'>" +
               $"<polyline points='{string.Join(" ", pts)}' fill='none' stroke='{Blue}' stroke-width='2' stroke-linejoin='round'/>" +
               "</svg>";
    }
}
