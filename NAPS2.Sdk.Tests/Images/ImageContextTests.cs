using System.Drawing;
using NAPS2.Images.Gdi;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class ImageContextTests : ContextualTests
{
    [Fact]
    public void SaveSmallestFormat_BlackAndWhite()
    {
        var ctx = new GdiImageContext();
        var bw = ctx.PerformTransform(new GdiImage(ImageResources.color_image_bw), new BlackWhiteTransform());
        var path = Path.Combine(FolderPath, "test");

        var fullPath = ctx.SaveSmallestFormat(path, bw, BitDepth.BlackAndWhite, false, -1, out var format);

        AssertPng(format, fullPath, ImageResources.color_image_bw);
    }

    [Fact]
    public void SaveSmallestFormat_BlackAndWhiteWithColorBitDepth()
    {
        var ctx = new GdiImageContext();
        var bw = ctx.PerformTransform(new GdiImage(ImageResources.color_image_bw), new BlackWhiteTransform());
        var path = Path.Combine(FolderPath, "test");

        var fullPath = ctx.SaveSmallestFormat(path, bw, BitDepth.Color, false, -1, out var format);

        AssertPng(format, fullPath, ImageResources.color_image_bw);
    }

    [Fact]
    public void SaveSmallestFormat_ColorWithBlackWhiteBitDepth()
    {
        var ctx = new GdiImageContext();
        var color = new GdiImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test");

        var fullPath = ctx.SaveSmallestFormat(path, color, BitDepth.BlackAndWhite, false, -1, out var format);

        AssertPng(format, fullPath, ImageResources.color_image_bw);
    }

    [Fact]
    public void SaveSmallestFormat_ColorHighQuality()
    {
        var ctx = new GdiImageContext();
        var color = new GdiImage(ImageResources.color_image);
        var path = Path.Combine(FolderPath, "test");

        var fullPath = ctx.SaveSmallestFormat(path, color, BitDepth.Color, true, -1, out var format);

        AssertPng(format, fullPath, ImageResources.color_image);
    }

    [Fact]
    public void SaveSmallestFormat_SmallerPng()
    {
        var ctx = new GdiImageContext();
        var bw = new GdiImage(ImageResources.color_image_bw_24bit);
        var path = Path.Combine(FolderPath, "test");

        var fullPath = ctx.SaveSmallestFormat(path, bw, BitDepth.Color, false, -1, out var format);

        AssertPng(format, fullPath, ImageResources.color_image_bw);
    }

    [Fact]
    public void SaveSmallestFormat_SmallerJpeg()
    {
        var ctx = new GdiImageContext();
        var color = new GdiImage(ImageResources.color_image_png);
        var path = Path.Combine(FolderPath, "test");

        var fullPath = ctx.SaveSmallestFormat(path, color, BitDepth.Color, false, -1, out var format);

        AssertJpeg(format, fullPath, ImageResources.color_image);
    }

    [Fact]
    public void SaveSmallestFormat_OriginalJpeg()
    {
        var ctx = new GdiImageContext();
        var color = new GdiImage(ImageResources.color_image_bw_jpg);
        var path = Path.Combine(FolderPath, "test");

        var fullPath = ctx.SaveSmallestFormat(path, color, BitDepth.Color, false, -1, out var format);

        AssertJpeg(format, fullPath, ImageResources.color_image_bw);
    }

    private void AssertPng(ImageFileFormat format, string fullPath, Bitmap expectedImage)
    {
        Assert.Equal(ImageFileFormat.Png, format);
        Assert.Equal(".png", Path.GetExtension(fullPath));
        var loaded = ImageContext.Load(fullPath);
        Assert.Equal(ImageFileFormat.Png, loaded.OriginalFileFormat);
        ImageAsserts.Similar(expectedImage, loaded, ignoreFormat: true);
    }

    private void AssertJpeg(ImageFileFormat format, string fullPath, Bitmap expectedImage)
    {
        Assert.Equal(ImageFileFormat.Jpeg, format);
        Assert.Equal(".jpg", Path.GetExtension(fullPath));
        var loaded = ImageContext.Load(fullPath);
        Assert.Equal(ImageFileFormat.Jpeg, loaded.OriginalFileFormat);
        ImageAsserts.Similar(expectedImage, loaded, ignoreFormat: true);
    }
}