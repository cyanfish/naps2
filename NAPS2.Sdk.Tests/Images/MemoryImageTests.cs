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
        var image = LoadImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test.jpg");
        
        image.Save(path, ImageFileFormat.Jpeg);

        var loaded = TestImageContextFactory.Get().Load(path);
        ImageAsserts.Similar(ImageResources.color_image, loaded);
    }

    [Fact]
    public void SaveWithQuality()
    {
        var image = LoadImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test.jpg");
        
        image.Save(path, ImageFileFormat.Jpeg, 25);

        var loaded = TestImageContextFactory.Get().Load(path);
        // Rather than comparing to a reference image (which doesn't work consistently cross-platform), we just assert
        // that we're a little bit off from the original image. i.e. that quality does *something*
        ImageAsserts.NotSimilar(ImageResources.color_image, loaded);
        ImageAsserts.Similar(ImageResources.color_image, loaded, 5.0);
    }

    [Fact]
    public void SaveToStream()
    {
        var image = LoadImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test.jpg");

        using var stream = new FileStream(path, FileMode.CreateNew);
        image.Save(stream, ImageFileFormat.Jpeg);

        var loaded = TestImageContextFactory.Get().Load(stream);
        ImageAsserts.Similar(ImageResources.color_image, loaded);
    }

    [Fact]
    public void SaveWithQualityToStream()
    {
        var image = LoadImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test.jpg");
        
        using var stream = new FileStream(path, FileMode.CreateNew);
        image.Save(stream, ImageFileFormat.Jpeg, 25);

        var loaded = TestImageContextFactory.Get().Load(stream);
        // Rather than comparing to a reference image (which doesn't work consistently cross-platform), we just assert
        // that we're a little bit off from the original image. i.e. that quality does *something*
        ImageAsserts.NotSimilar(ImageResources.color_image, loaded);
        ImageAsserts.Similar(ImageResources.color_image, loaded, 5.0);
    }

    [Fact]
    public void SaveWithUnspecifiedFormatToPng()
    {
        var image = LoadImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test.png");
        
        image.Save(path);

        var loaded = TestImageContextFactory.Get().Load(path);
        Assert.Equal(ImageFileFormat.Png, loaded.OriginalFileFormat);
        ImageAsserts.Similar(ImageResources.color_image, loaded, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SaveWithUnspecifiedFormatToJpeg()
    {
        var image = LoadImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test.jpg");
        
        image.Save(path);

        var loaded = TestImageContextFactory.Get().Load(path);
        Assert.Equal(ImageFileFormat.Jpeg, loaded.OriginalFileFormat);
    }

    [Fact]
    public void SaveWithUnspecifiedFormatToStream()
    {
        var image = LoadImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test.png");
        
        using var stream = new FileStream(path, FileMode.CreateNew);
        Assert.Throws<ArgumentException>(() => image.Save(stream, ImageFileFormat.Unspecified));
    }
}