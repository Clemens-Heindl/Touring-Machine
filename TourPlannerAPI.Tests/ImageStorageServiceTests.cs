using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TourPlannerAPI.Configuration;
using TourPlannerAPI.Exceptions;
using TourPlannerAPI.Services;

namespace TourPlannerAPI.Tests;

[TestFixture]
public class ImageStorageServiceTests
{
    private string _tempDir = null!;
    private ImageStorageService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "tp-image-tests", Guid.NewGuid().ToString("N"));
        var options = Options.Create(new ImageStorageOptions
        {
            BaseDirectory = _tempDir,
            MaxSizeBytes = 1024
        });
        _service = new ImageStorageService(options, NullLogger<ImageStorageService>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private static IFormFile FakeFile(string contentType, int size)
    {
        var stream = new MemoryStream(new byte[size]);
        return new FormFile(stream, 0, stream.Length, "file", "upload")
        {
            Headers = new HeaderDictionary { { "Content-Type", contentType } }
        };
    }

    [Test]
    public async Task SaveAsync_StoresFile_AndReturnsGeneratedName()
    {
        var fileName = await _service.SaveAsync(FakeFile("image/png", 200));

        Assert.That(fileName, Does.EndWith(".png"));
        Assert.That(File.Exists(Path.Combine(_tempDir, fileName)), Is.True);
    }

    [Test]
    public void SaveAsync_Throws_ForUnsupportedType()
    {
        Assert.That(async () => await _service.SaveAsync(FakeFile("text/plain", 200)),
            Throws.TypeOf<ValidationException>());
    }

    [Test]
    public void SaveAsync_Throws_ForOversizedFile()
    {
        Assert.That(async () => await _service.SaveAsync(FakeFile("image/png", 5000)),
            Throws.TypeOf<ValidationException>());
    }

    [Test]
    public void SaveAsync_Throws_ForEmptyFile()
    {
        Assert.That(async () => await _service.SaveAsync(FakeFile("image/png", 0)),
            Throws.TypeOf<ValidationException>());
    }

    [Test]
    public void Open_ReturnsNull_ForPathTraversalAttempt()
    {
        Assert.That(_service.Open("../secret.png"), Is.Null);
    }

    [Test]
    public void Open_ReturnsNull_ForMissingFile()
    {
        Assert.That(_service.Open("does-not-exist.png"), Is.Null);
    }
}
