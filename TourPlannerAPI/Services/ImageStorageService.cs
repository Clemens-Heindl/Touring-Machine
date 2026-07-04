using Microsoft.Extensions.Options;
using TourPlannerAPI.Configuration;
using TourPlannerAPI.Exceptions;

namespace TourPlannerAPI.Services;

/// <summary>Filesystem implementation of <see cref="IImageStorageService"/>.</summary>
public class ImageStorageService : IImageStorageService
{
    private static readonly Dictionary<string, string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "image/jpeg", ".jpg" },
        { "image/png", ".png" },
        { "image/webp", ".webp" },
        { "image/gif", ".gif" }
    };

    private readonly ImageStorageOptions _options;
    private readonly ILogger<ImageStorageService> _logger;

    public ImageStorageService(IOptions<ImageStorageOptions> options, ILogger<ImageStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
        Directory.CreateDirectory(_options.BaseDirectory);
    }

    public async Task<string> SaveAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new ValidationException("No image file was provided.");

        if (file.Length > _options.MaxSizeBytes)
            throw new ValidationException($"Image exceeds the maximum size of {_options.MaxSizeBytes / (1024 * 1024)} MB.");

        if (!AllowedTypes.TryGetValue(file.ContentType, out var extension))
            throw new ValidationException("Unsupported image type. Allowed: JPEG, PNG, WebP, GIF.");

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_options.BaseDirectory, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.CreateNew))
        {
            await file.CopyToAsync(stream);
        }

        _logger.LogInformation("Stored image {FileName} ({Bytes} bytes)", fileName, file.Length);
        return fileName;
    }

    public (Stream Stream, string ContentType)? Open(string fileName)
    {
        // Guard against path traversal: only a bare file name is allowed.
        if (string.IsNullOrWhiteSpace(fileName) || fileName != Path.GetFileName(fileName))
            return null;

        var fullPath = Path.Combine(_options.BaseDirectory, fileName);
        if (!File.Exists(fullPath))
            return null;

        var extension = Path.GetExtension(fileName);
        var contentType = AllowedTypes.FirstOrDefault(kv => kv.Value.Equals(extension, StringComparison.OrdinalIgnoreCase)).Key
            ?? "application/octet-stream";

        return (File.OpenRead(fullPath), contentType);
    }
}
