using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class ImageExportHelperTests : ContextualTests
{
    private ImageExportHelper _helper;

    public ImageExportHelperTests()
    {
        _helper = new ImageExportHelper();
    }

    [Fact]
    public void SaveSmallestFormat_BlackAndWhite()
    {
        var bw = LoadImage(ImageResources.dog_bw).PerformTransform(new BlackWhiteTransform());
        var path = Path.Combine(FolderPath, "test");

        var fullPath = _helper.SaveSmallestFormat(path, bw, false, -1, out var format);

        AssertPng(format, fullPath, ImageResources.dog_bw);
    }

    [Fact]
    public void SaveSmallestFormat_BlackAndWhiteWithColorBitDepth()
    {
        var bw = LoadImage(ImageResources.dog_bw).PerformTransform(new BlackWhiteTransform());
        var path = Path.Combine(FolderPath, "test");

        var fullPath = _helper.SaveSmallestFormat(path, bw, false, -1, out var format);

        AssertPng(format, fullPath, ImageResources.dog_bw);
    }

    [Fact]
    public void SaveSmallestFormat_ColorHighQuality()
    {
        var color = LoadImage(ImageResources.dog);
        var path = Path.Combine(FolderPath, "test");

        var fullPath = _helper.SaveSmallestFormat(path, color, true, -1, out var format);

        AssertPng(format, fullPath, ImageResources.dog);
    }

    [Fact]
    public void SaveSmallestFormat_LogicalBlackWhite()
    {
        var bw = LoadImage(ImageResources.dog_bw_24bit);
        var path = Path.Combine(FolderPath, "test");

        var fullPath = _helper.SaveSmallestFormat(path, bw, false, -1, out var format);

        AssertPng(format, fullPath, ImageResources.dog_bw);
    }

    [Fact]
    public void SaveSmallestFormat_SmallerPng()
    {
        var bw = LoadImage(ImageResources.dog_clustered_gray);
        var path = Path.Combine(FolderPath, "test");

        var fullPath = _helper.SaveSmallestFormat(path, bw, false, -1, out var format);

        AssertPng(format, fullPath, ImageResources.dog_clustered_gray);
    }

    [Fact]
    public void SaveSmallestFormat_OriginalPng()
    {
        var color = LoadImage(ImageResources.dog_png);
        var path = Path.Combine(FolderPath, "test");

        var fullPath = _helper.SaveSmallestFormat(path, color, false, -1, out var format);

        AssertPng(format, fullPath, ImageResources.dog);
    }

    [Fact]
    public void SaveSmallestFormat_SmallerJpeg()
    {
        var color = LoadImage(ImageResources.dog_png);
        color.OriginalFileFormat = ImageFileFormat.Unknown;
        var path = Path.Combine(FolderPath, "test");

        var fullPath = _helper.SaveSmallestFormat(path, color, false, -1, out var format);

        AssertJpeg(format, fullPath, ImageResources.dog);
    }

    [Fact]
    public void SaveSmallestFormat_OriginalJpeg()
    {
        var color = LoadImage(ImageResources.dog_bw_jpg);
        var path = Path.Combine(FolderPath, "test");

        var fullPath = _helper.SaveSmallestFormat(path, color, false, -1, out var format);

        AssertJpeg(format, fullPath, ImageResources.dog_bw);
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
        ImageAsserts.Similar(expectedImage, loaded, ImageAsserts.XPLAT_RMSE_THRESHOLD);
    }
}