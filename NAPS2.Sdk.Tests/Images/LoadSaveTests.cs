using System.Globalization;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class LoadSaveTests : ContextualTests
{
    // TODO: Add tests for error/edge cases (e.g. invalid files, unicode file names)

    [Theory]
    [MemberData(nameof(TestCases))]
    public void LoadFromFile(ImageFileFormat format, string ext, string resource, string[] compare,
        ImagePixelFormat[] logicalPixelFormats, bool ignoreRes)
    {
        var path = CopyResourceToFile(GetResource(resource), $"image{ext}");
        using var image = ImageContext.Load(path);
        Assert.Equal(format, image.OriginalFileFormat);
        Assert.Equal(logicalPixelFormats[0], image.LogicalPixelFormat);
        ImageAsserts.Similar(GetResource(compare[0]), image, ignoreResolution: ignoreRes);
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void LoadFromStream(ImageFileFormat format, string ext, string resource, string[] compare,
        ImagePixelFormat[] logicalPixelFormats, bool ignoreRes)
    {
        var stream = new MemoryStream(GetResource(resource));
        using var image = ImageContext.Load(stream);
        Assert.Equal(format, image.OriginalFileFormat);
        Assert.Equal(logicalPixelFormats[0], image.LogicalPixelFormat);
        ImageAsserts.Similar(GetResource(compare[0]), image, ignoreResolution: ignoreRes);
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void LoadFramesFromFile(ImageFileFormat format, string ext, string resource, string[] compare,
        ImagePixelFormat[] logicalPixelFormats, bool ignoreRes)
    {
        var path = CopyResourceToFile(GetResource(resource), $"image{ext}");
        var images = ImageContext.LoadFrames(path, out var count).ToArray();
        Assert.Equal(compare.Length, count);
        Assert.Equal(compare.Length, images.Length);
        for (int i = 0; i < images.Length; i++)
        {
            Assert.Equal(format, images[i].OriginalFileFormat);
            Assert.Equal(logicalPixelFormats[i], images[i].LogicalPixelFormat);
            ImageAsserts.Similar(GetResource(compare[i]), images[i], ignoreResolution: ignoreRes);
        }
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void LoadFramesFromStream(ImageFileFormat format, string ext, string resource, string[] compare,
        ImagePixelFormat[] logicalPixelFormats, bool ignoreRes)
    {
        var stream = new MemoryStream(GetResource(resource));
        var images = ImageContext.LoadFrames(stream, out var count).ToArray();
        Assert.Equal(compare.Length, count);
        Assert.Equal(compare.Length, images.Length);
        for (int i = 0; i < images.Length; i++)
        {
            Assert.Equal(format, images[i].OriginalFileFormat);
            Assert.Equal(logicalPixelFormats[i], images[i].LogicalPixelFormat);
            ImageAsserts.Similar(GetResource(compare[i]), images[i], ignoreResolution: ignoreRes);
        }
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void SaveToFile(ImageFileFormat format, string ext, string resource, string[] compare,
        ImagePixelFormat[] logicalPixelFormats, bool ignoreRes)
    {
        var image = LoadImage(GetResource(resource));
        var path = Path.Combine(FolderPath, $"image{ext}");
        image.Save(path);
        var image2 = ImageContext.Load(path);
        Assert.Equal(format, image2.OriginalFileFormat);
        Assert.Equal(logicalPixelFormats[0], image2.LogicalPixelFormat);
        ImageAsserts.Similar(GetResource(compare[0]), image2, ignoreResolution: ignoreRes);
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void SaveToStream(ImageFileFormat format, string ext, string resource, string[] compare,
        ImagePixelFormat[] logicalPixelFormats, bool ignoreRes)
    {
        var image = LoadImage(GetResource(resource));
        var stream = new MemoryStream();
        image.Save(stream, format);
        var image2 = ImageContext.Load(stream);
        Assert.Equal(format, image2.OriginalFileFormat);
        Assert.Equal(logicalPixelFormats[0], image2.LogicalPixelFormat);
        ImageAsserts.Similar(GetResource(compare[0]), image2, ignoreResolution: ignoreRes);
    }

    [Fact]
    public void LoadFromWrongExtension()
    {
        // Actually a jpeg
        var path = CopyResourceToFile(ImageResources.dog, "image.png");
        var image = ImageContext.Load(path);
        Assert.Equal(ImageFileFormat.Jpeg, image.OriginalFileFormat);
        ImageAsserts.Similar(ImageResources.dog, image);
    }

    [Fact]
    public void LoadFramesFromWrongExtension()
    {
        // Actually a jpeg
        var path = CopyResourceToFile(ImageResources.dog, "image.tiff");
        var images = ImageContext.LoadFrames(path, out _).ToList();
        Assert.Single(images);
        Assert.Equal(ImageFileFormat.Jpeg, images[0].OriginalFileFormat);
        ImageAsserts.Similar(ImageResources.dog, images[0]);
    }

    private static byte[] GetResource(string resource) =>
        (byte[]) ImageResources.ResourceManager.GetObject(resource, CultureInfo.InvariantCulture);

    // TODO: Ignore resolution by default in the existing tests, but have separate tests/test cases for resolution
    public static IEnumerable<object[]> TestCases = new List<object[]>
    {
        new object[]
        {
            ImageFileFormat.Png, ".png", "dog_alpha",
            new[] { "dog_alpha" }, new[] { ImagePixelFormat.ARGB32 }, false
        },
        new object[]
        {
            ImageFileFormat.Png, ".png", "dog_png",
            new[] { "dog" }, new[] { ImagePixelFormat.RGB24 }, false
        },
        new object[]
        {
            ImageFileFormat.Png, ".png", "dog_bw",
            new[] { "dog_bw" }, new[] { ImagePixelFormat.BW1 }, false
        },
        // TODO: Update resources for more pixel format tests
        new object[]
        {
            ImageFileFormat.Jpeg, ".jpg", "dog",
            new[] { "dog" }, new[] { ImagePixelFormat.RGB24 }, false
        },
        new object[]
        {
            ImageFileFormat.Jpeg, ".jpg", "dog_bw_jpg",
            new[] { "dog_bw" }, new[] { ImagePixelFormat.Gray8 }, false
        },
        new object[]
        {
            ImageFileFormat.Bmp, ".bmp", "dog_bw_invertpal",
            new[] { "dog_bw" }, new[] { ImagePixelFormat.BW1 }, true
        },
        new object[]
        {
            ImageFileFormat.Tiff, ".tiff", "dog_tiff",
            new[] { "dog", "dog_h_p300", "stock_cat" },
            new[] { ImagePixelFormat.RGB24, ImagePixelFormat.RGB24, ImagePixelFormat.RGB24 }, false
        },
    };
}