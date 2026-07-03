namespace TourPlannerAPI.Services;

/// <summary>Stores and retrieves tour images on the filesystem.</summary>
public interface IImageStorageService
{
    /// <summary>Validates and saves an uploaded image, returning its stored file name.</summary>
    Task<string> SaveAsync(IFormFile file);

    /// <summary>Opens a stored image for reading, or null if it does not exist.</summary>
    (Stream Stream, string ContentType)? Open(string fileName);
}
