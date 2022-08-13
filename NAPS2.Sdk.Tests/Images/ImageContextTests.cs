using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class ImageContextTests : ContextualTests
{
    [Fact]
    public void SaveSmallestFormat_BlackAndWhite()
    {
        var ctx = TestImageContextFactory.Get();
        var bw = ctx.PerformTransform(LoadImage(ImageResources.color_image_bw), new BlackWhiteTransform());
        var path = Path.Combine(FolderPath, "test");

        var fullPath = ctx.SaveSmallestFormat(path, bw, BitDepth.BlackAndWhite, false, -1, out var format);

        AssertPng(format, fullPath, ImageResources.color_image_bw);
    }

    [Fact]
    public void SaveSmallestFormat_BlackAndWhiteWithColorBitDepth()
    {
        var ctx = TestImageContextFactory.Get();
        var bw = ctx.PerformTransform(LoadImage(ImageResources.color_image_bw), new BlackWhiteTransform());
        var path = Path.Combine(FolderPath, "test");

        var fullPath = ctx.SaveSmallestFormat(path, bw, BitDepth.Color, false, -1, out var format);

        AssertPng(format, fullPath, ImageResources.color_image_bw);
    }

    [Fact]
    public void SaveSmallestFormat_ColorWithBlackWhiteBitDepth()
    {
        var ctx = TestImageContextFactory.Get();
        var color = LoadImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test");

        var fullPath = ctx.SaveSmallestFormat(path, color, BitDepth.BlackAndWhite, false, -1, out var format);

        AssertPng(format, fullPath, ImageResources.color_image_bw, ImageAsserts.XPLAT_RMSE_THRESHOLD);
    }

    [Fact]
    public void SaveSmallestFormat_ColorHighQuality()
    {
        var ctx = TestImageContextFactory.Get();
        var color = LoadImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test");

        var fullPath = ctx.SaveSmallestFormat(path, color, BitDepth.Color, true, -1, out var format);

        AssertPng(format, fullPath, ImageResources.color_image);
    }

    [Fact]
    public void SaveSmallestFormat_SmallerPng()
    {
        var ctx = TestImageContextFactory.Get();
        var bw = LoadImage(ImageResources.color_image_bw_24bit);
        var path = Path.Combine(FolderPath, "test");

        var fullPath = ctx.SaveSmallestFormat(path, bw, BitDepth.Color, false, -1, out var format);

        AssertPng(format, fullPath, ImageResources.color_image_bw);
    }

    [Fact]
    public void SaveSmallestFormat_OriginalPng()
    {
        var ctx = TestImageContextFactory.Get();
        var color = LoadImage(ImageResources.color_image_png);
        var path = Path.Combine(FolderPath, "test");

        var fullPath = ctx.SaveSmallestFormat(path, color, BitDepth.Color, false, -1, out var format);

        AssertPng(format, fullPath, ImageResources.color_image);
    }

    [Fact]
    public void SaveSmallestFormat_SmallerJpeg()
    {
        var ctx = TestImageContextFactory.Get();
        var color = LoadImage(ImageResources.color_image_png);
        color.OriginalFileFormat = ImageFileFormat.Unspecified;
        var path = Path.Combine(FolderPath, "test");

        var fullPath = ctx.SaveSmallestFormat(path, color, BitDepth.Color, false, -1, out var format);

        AssertJpeg(format, fullPath, ImageResources.color_image);
    }

    [Fact]
    public void SaveSmallestFormat_OriginalJpeg()
    {
        var ctx = TestImageContextFactory.Get();
        var color = LoadImage(ImageResources.color_image_bw_jpg);
        var path = Path.Combine(FolderPath, "test");

        var fullPath = ctx.SaveSmallestFormat(path, color, BitDepth.Color, false, -1, out var format);

        AssertJpeg(format, fullPath, ImageResources.color_image_bw);
    }

    private void AssertPng(ImageFileFormat format, string fullPath, byte[] expectedImage,
        double rmseThreshold = ImageAsserts.GENERAL_RMSE_THRESHOLD)
    {
        Assert.Equal(ImageFileFormat.Png, format);
        Assert.Equal(".png", Path.GetExtension(fullPath));
        var loaded = ImageContext.Load(fullPath);
        Assert.Equal(ImageFileFormat.Png, loaded.OriginalFileFormat);
        ImageAsserts.Similar(expectedImage, loaded, rmseThreshold);
    }

    private void AssertJpeg(ImageFileFormat format, string fullPath, byte[] expectedImage)
    {
        Assert.Equal(ImageFileFormat.Jpeg, format);
        Assert.Equal(".jpg", Path.GetExtension(fullPath));
        var loaded = ImageContext.Load(fullPath);
        Assert.Equal(ImageFileFormat.Jpeg, loaded.OriginalFileFormat);
        ImageAsserts.Similar(expectedImage, loaded);
    }
}