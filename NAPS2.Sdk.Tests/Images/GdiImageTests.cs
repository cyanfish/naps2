using System.Drawing;
using NAPS2.Images.Gdi;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class GdiImageTests : ContextualTests
{
    [Fact]
    public void LockAndDisposeTwice()
    {
        var image = new GdiImage(new Bitmap(100, 100));
        var lockState = image.Lock(LockMode.ReadWrite, out var scan0, out var stride);
        lockState.Dispose();
        lockState.Dispose();
    }

    [Fact]
    public void SaveWithoutQuality()
    {
        var image = new GdiImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test.jpg");
        
        image.Save(path, ImageFileFormat.Jpeg);

        var loaded = new GdiImageContext().Load(path);
        ImageAsserts.Similar(ImageResources.color_image, loaded);
    }

    [Fact]
    public void SaveWithQuality()
    {
        var image = new GdiImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test.jpg");
        
        image.Save(path, ImageFileFormat.Jpeg, 10);

        var loaded = new GdiImageContext().Load(path);
        ImageAsserts.Similar(ImageResources.color_image_low_quality, loaded);
    }

    [Fact]
    public void SaveWithoutQualityToStream()
    {
        var image = new GdiImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test.jpg");

        using var stream = new FileStream(path, FileMode.CreateNew);
        image.Save(stream, ImageFileFormat.Jpeg);

        var loaded = new GdiImageContext().Load(stream);
        ImageAsserts.Similar(ImageResources.color_image, loaded);
    }

    [Fact]
    public void SaveWithQualityToStream()
    {
        var image = new GdiImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test.jpg");
        
        using var stream = new FileStream(path, FileMode.CreateNew);
        image.Save(stream, ImageFileFormat.Jpeg, 10);

        var loaded = new GdiImageContext().Load(stream);
        ImageAsserts.Similar(ImageResources.color_image_low_quality, loaded);
    }

    [Fact]
    public void SaveWithUnspecifiedFormatToPng()
    {
        var image = new GdiImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test.png");
        
        image.Save(path);

        var loaded = new GdiImageContext().Load(path);
        Assert.Equal(ImageFileFormat.Png, loaded.OriginalFileFormat);
        ImageAsserts.Similar(ImageResources.color_image, loaded, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SaveWithUnspecifiedFormatToJpeg()
    {
        var image = new GdiImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test.jpg");
        
        image.Save(path);

        var loaded = new GdiImageContext().Load(path);
        Assert.Equal(ImageFileFormat.Jpeg, loaded.OriginalFileFormat);
    }

    [Fact]
    public void SaveWithUnspecifiedFormatToStream()
    {
        var image = new GdiImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test.png");
        
        using var stream = new FileStream(path, FileMode.CreateNew);
        Assert.Throws<ArgumentException>(() => image.Save(stream, ImageFileFormat.Unspecified));
    }
}