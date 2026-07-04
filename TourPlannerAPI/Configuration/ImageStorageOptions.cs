namespace TourPlannerAPI.Configuration;

/// <summary>
/// Configuration for filesystem image storage. Images are stored externally on
/// disk (only the file name is kept in the database), per the project spec.
/// </summary>
public class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    /// <summary>Base directory where uploaded images are written.</summary>
    public string BaseDirectory { get; set; } = "ImageStore";

    /// <summary>Maximum accepted upload size in bytes.</summary>
    public long MaxSizeBytes { get; set; } = 5 * 1024 * 1024;
}
