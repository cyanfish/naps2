using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class BundledCodecTests : ContextualTests
{
    [Fact]
    public void RegisteredByScanningContext()
    {
        Assert.IsType<WebPCodec>(ImageContext.BundledEncoders[ImageFileFormat.WebP]);
        Assert.IsType<WebPCodec>(ImageContext.BundledDecoders[ImageFileFormat.WebP]);
        Assert.IsType<JpegXlCodec>(ImageContext.BundledEncoders[ImageFileFormat.JpegXl]);
        Assert.IsType<JpegXlCodec>(ImageContext.BundledDecoders[ImageFileFormat.JpegXl]);
        Assert.True(ImageContext.SupportsFormat(ImageFileFormat.WebP));
        Assert.True(ImageContext.SupportsFormat(ImageFileFormat.JpegXl));
    }

    [Theory]
    [InlineData(ImageFileFormat.WebP)]
    [InlineData(ImageFileFormat.JpegXl)]
    public void SavesResolution(ImageFileFormat format)
    {
        using var image = LoadImage(ImageResources.dog);
        image.SetResolution(300, 150.5f);

        using var image2 = ImageContext.Load(image.SaveToMemoryStream(format));

        Assert.Equal(300, image2.HorizontalResolution, 2);
        Assert.Equal(150.5, image2.VerticalResolution, 2);
    }

    [Theory]
    [InlineData(ImageFileFormat.WebP)]
    [InlineData(ImageFileFormat.JpegXl)]
    public void LosslessForBlackAndWhite(ImageFileFormat format)
    {
        using var image = LoadImage(ImageResources.dog_bw);

        var stream = image.SaveToMemoryStream(format);

        ImageAsserts.Similar(image, ImageContext.Load(stream), ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void QualityAffectsSize()
    {
        using var image = LoadImage(ImageResources.dog);

        foreach (var format in new[] { ImageFileFormat.WebP, ImageFileFormat.JpegXl })
        {
            var low = image.SaveToMemoryStream(format, new ImageSaveOptions { Quality = 30 });
            var high = image.SaveToMemoryStream(format, new ImageSaveOptions { Quality = 95 });
            Assert.True(low.Length < high.Length);
        }
    }

    [Fact]
    public void WebpTooBigThrows()
    {
        using var image = ImageContext.Create(16384, 1, ImagePixelFormat.RGB24);

        var ex = Assert.Throws<InvalidOperationException>(() => image.SaveToMemoryStream(ImageFileFormat.WebP));
        Assert.Contains("16383", ex.Message);
    }

    [Fact]
    public void InvalidDataThrows()
    {
        // Valid signatures followed by garbage
        byte[] webp = [.."RIFF\0\0\0\0WEBPVP8 "u8.ToArray(), ..new byte[32]];
        byte[] jxl = [0xFF, 0x0A, ..new byte[32]];

        Assert.Throws<InvalidOperationException>(() => ImageContext.Load(webp));
        Assert.Throws<InvalidOperationException>(() => ImageContext.Load(jxl));
    }
}
