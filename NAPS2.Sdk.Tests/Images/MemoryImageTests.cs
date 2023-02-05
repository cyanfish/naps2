using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class MemoryImageTests : ContextualTests
{
    [Fact]
    public void LockAndDisposeTwice()
    {
        var image = TestImageContextFactory.Get().Create(100, 100, ImagePixelFormat.RGB24);
        var lockState = image.Lock(LockMode.ReadWrite, out var data);
        lockState.Dispose();
        lockState.Dispose();
    }

    [Fact]
    public void Save()
    {
        var image = LoadImage(ImageResources.dog);
        var path = Path.Combine(FolderPath, "test.jpg");
        
        image.Save(path, ImageFileFormat.Jpeg);

        var loaded = TestImageContextFactory.Get().Load(path);
        ImageAsserts.Similar(ImageResources.dog, loaded);
    }

    [Fact]
    public void SaveWithQuality()
    {
        var image = LoadImage(ImageResources.dog);
        var highQualityPath = Path.Combine(FolderPath, "highq.jpg");
        var lowQualityPath = Path.Combine(FolderPath, "lowq.jpg");
        
        image.Save(highQualityPath, ImageFileFormat.Jpeg, new ImageSaveOptions { Quality = 75 });
        image.Save(lowQualityPath, ImageFileFormat.Jpeg, new ImageSaveOptions { Quality = 25 });

        var highQuality = TestImageContextFactory.Get().Load(highQualityPath);
        var lowQuality = TestImageContextFactory.Get().Load(lowQualityPath);

        ImageAsserts.Similar(ImageResources.dog, highQuality);
        // Rather than comparing to a reference image (which doesn't work consistently cross-platform), we just assert
        // that we're a little bit off from the original image. i.e. that quality does *something*
        ImageAsserts.NotSimilar(ImageResources.dog, lowQuality);
        ImageAsserts.Similar(ImageResources.dog, lowQuality, 5.0);
    }

    [Fact]
    public void SaveToStream()
    {
        var image = LoadImage(ImageResources.dog);
        var path = Path.Combine(FolderPath, "test.jpg");

        using var stream = new FileStream(path, FileMode.CreateNew);
        image.Save(stream, ImageFileFormat.Jpeg);

        var loaded = TestImageContextFactory.Get().Load(stream);
        ImageAsserts.Similar(ImageResources.dog, loaded);
    }

    [Fact]
    public void SaveWithQualityToStream()
    {
        var image = LoadImage(ImageResources.dog);
        var highQualityStream = new MemoryStream();
        var lowQualityStream = new MemoryStream();

        image.Save(highQualityStream, ImageFileFormat.Jpeg, new ImageSaveOptions { Quality = 75 });
        image.Save(lowQualityStream, ImageFileFormat.Jpeg, new ImageSaveOptions { Quality = 25 });

        var highQuality = TestImageContextFactory.Get().Load(highQualityStream);
        var lowQuality = TestImageContextFactory.Get().Load(lowQualityStream);

        ImageAsserts.Similar(ImageResources.dog, highQuality);
        // Rather than comparing to a reference image (which doesn't work consistently cross-platform), we just assert
        // that we're a little bit off from the original image. i.e. that quality does *something*
        ImageAsserts.NotSimilar(ImageResources.dog, lowQuality);
        ImageAsserts.Similar(ImageResources.dog, lowQuality, 5.0);
    }

    [Fact]
    public void SaveWithUnspecifiedFormatToPng()
    {
        var image = LoadImage(ImageResources.dog);
        var path = Path.Combine(FolderPath, "test.png");
        
        image.Save(path);

        var loaded = TestImageContextFactory.Get().Load(path);
        Assert.Equal(ImageFileFormat.Png, loaded.OriginalFileFormat);
        ImageAsserts.Similar(ImageResources.dog, loaded, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SaveWithUnspecifiedFormatToJpeg()
    {
        var image = LoadImage(ImageResources.dog);
        var path = Path.Combine(FolderPath, "test.jpg");
        
        image.Save(path);

        var loaded = TestImageContextFactory.Get().Load(path);
        Assert.Equal(ImageFileFormat.Jpeg, loaded.OriginalFileFormat);
    }

    [Fact]
    public void SaveWithUnspecifiedFormatToStream()
    {
        var image = LoadImage(ImageResources.dog);
        var path = Path.Combine(FolderPath, "test.png");
        
        using var stream = new FileStream(path, FileMode.CreateNew);
        Assert.Throws<ArgumentException>(() => image.Save(stream, ImageFileFormat.Unspecified));
    }
}