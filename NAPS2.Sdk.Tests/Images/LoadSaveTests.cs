using System.Globalization;
using Moq;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

// As we use the same data for multiple methods, some parameters may be unused
#pragma warning disable xUnit1026

public class LoadSaveTests : ContextualTests
{
    // TODO: Add tests for error/edge cases (e.g. invalid files, unicode file names)

    [Theory]
    [MemberData(nameof(TestCases))]
    public void LoadFromFile(ImageFileFormat format, string ext, string resource, string[] compare,
        ImagePixelFormat[] logicalPixelFormats, bool ignoreRes)
    {
        var path = CopyResourceToFile(GetResource(resource), $"image{ext}");

        if (!ImageContext.SupportsFormat(format))
        {
            Assert.Throws<NotSupportedException>(() => ImageContext.Load(path));
            return;
        }
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

        if (!ImageContext.SupportsFormat(format))
        {
            Assert.Throws<NotSupportedException>(() => ImageContext.Load(stream));
            return;
        }
        using var image = ImageContext.Load(stream);

        Assert.Equal(format, image.OriginalFileFormat);
        Assert.Equal(logicalPixelFormats[0], image.LogicalPixelFormat);
        ImageAsserts.Similar(GetResource(compare[0]), image, ignoreResolution: ignoreRes);
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task LoadFramesFromFile(ImageFileFormat format, string ext, string resource, string[] compare,
        ImagePixelFormat[] logicalPixelFormats, bool ignoreRes)
    {
        var path = CopyResourceToFile(GetResource(resource), $"image{ext}");
        var progressMock = new Mock<ProgressCallback>();

        if (!ImageContext.SupportsFormat(format))
        {
            await Assert.ThrowsAsync<NotSupportedException>(async () =>
                await ImageContext.LoadFrames(path, progressMock.Object).ToListAsync());
            return;
        }
        var images = await ImageContext.LoadFrames(path, progressMock.Object).ToListAsync();

        Assert.Equal(compare.Length, images.Count);
        for (int i = 0; i < images.Count; i++)
        {
            Assert.Equal(format, images[i].OriginalFileFormat);
            Assert.Equal(logicalPixelFormats[i], images[i].LogicalPixelFormat);
            ImageAsserts.Similar(GetResource(compare[i]), images[i], ignoreResolution: ignoreRes);
            progressMock.Verify(x => x(i, images.Count));
        }
        progressMock.Verify(x => x(images.Count, images.Count));
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task LoadFramesFromStream(ImageFileFormat format, string ext, string resource, string[] compare,
        ImagePixelFormat[] logicalPixelFormats, bool ignoreRes)
    {
        var stream = new MemoryStream(GetResource(resource));
        var progressMock = new Mock<ProgressCallback>();

        if (!ImageContext.SupportsFormat(format))
        {
            await Assert.ThrowsAsync<NotSupportedException>(async () =>
                await ImageContext.LoadFrames(stream, progressMock.Object).ToListAsync());
            return;
        }
        var images = await ImageContext.LoadFrames(stream, progressMock.Object).ToListAsync();

        Assert.Equal(compare.Length, images.Count);
        for (int i = 0; i < images.Count; i++)
        {
            Assert.Equal(format, images[i].OriginalFileFormat);
            Assert.Equal(logicalPixelFormats[i], images[i].LogicalPixelFormat);
            ImageAsserts.Similar(GetResource(compare[i]), images[i], ignoreResolution: ignoreRes);
            progressMock.Verify(x => x(i, images.Count));
        }
        progressMock.Verify(x => x(images.Count, images.Count));
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void SaveToFile(ImageFileFormat format, string ext, string resource, string[] compare,
        ImagePixelFormat[] logicalPixelFormats, bool ignoreRes)
    {
        var path = Path.Combine(FolderPath, $"image{ext}");
        var expected = LoadImage(GetResource(compare[0]));

        if (!ImageContext.SupportsFormat(format))
        {
            Assert.Throws<NotSupportedException>(() => expected.Save(path));
            return;
        }
        var image = LoadImage(GetResource(resource));
        image.Save(path);

        var image2 = ImageContext.Load(path);
        Assert.Equal(format, image2.OriginalFileFormat);
        Assert.Equal(logicalPixelFormats[0], image2.LogicalPixelFormat);
        ImageAsserts.Similar(expected, image2, ignoreResolution: ignoreRes);
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void SaveToStream(ImageFileFormat format, string ext, string resource, string[] compare,
        ImagePixelFormat[] logicalPixelFormats, bool ignoreRes)
    {
        var stream = new MemoryStream();
        var expected = LoadImage(GetResource(compare[0]));

        if (!ImageContext.SupportsFormat(format))
        {
            Assert.Throws<NotSupportedException>(() => expected.Save(stream, format));
            return;
        }
        var image = LoadImage(GetResource(resource));
        image.Save(stream, format);

        var image2 = ImageContext.Load(stream);
        Assert.Equal(format, image2.OriginalFileFormat);
        Assert.Equal(logicalPixelFormats[0], image2.LogicalPixelFormat);
        ImageAsserts.Similar(expected, image2, ignoreResolution: ignoreRes);
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
    public async Task LoadFramesFromWrongExtension()
    {
        // Actually a jpeg
        var path = CopyResourceToFile(ImageResources.dog, "image.tiff");
        var images = await ImageContext.LoadFrames(path).ToListAsync();
        Assert.Single(images);
        Assert.Equal(ImageFileFormat.Jpeg, images[0].OriginalFileFormat);
        ImageAsserts.Similar(ImageResources.dog, images[0]);
    }

    [Fact]
    public void SetResolutionAndSaveJpeg()
    {
        var image = LoadImage(ImageResources.dog);

        image.SetResolution(300, 300);
        var stream = image.SaveToMemoryStream(ImageFileFormat.Jpeg);
        var image2 = ImageContext.Load(stream);

        Assert.Equal(300, image2.HorizontalResolution, 2);
        Assert.Equal(300, image2.VerticalResolution, 2);
    }

    [Fact]
    public void SetResolutionAndSavePng()
    {
        var image = LoadImage(ImageResources.dog);

        image.SetResolution(300, 300);
        var stream = image.SaveToMemoryStream(ImageFileFormat.Png);
        var image2 = ImageContext.Load(stream);

        Assert.Equal(300, image2.HorizontalResolution, 2);
        Assert.Equal(300, image2.VerticalResolution, 2);
    }

    [Fact]
    public void SavePngOptimizesBitDepth()
    {
        var image32Bpp = LoadImage(ImageResources.dog_bw).CopyWithPixelFormat(ImagePixelFormat.ARGB32);
        var image24Bpp = LoadImage(ImageResources.dog_bw).CopyWithPixelFormat(ImagePixelFormat.RGB24);
        var image8Bpp = LoadImage(ImageResources.dog_bw).CopyWithPixelFormat(ImagePixelFormat.Gray8);
        var image1Bpp = LoadImage(ImageResources.dog_bw).CopyWithPixelFormat(ImagePixelFormat.BW1);

        var optimized32Bpp = GetSavedSize(image24Bpp, ImageFileFormat.Png);
        var optimized24Bpp = GetSavedSize(image24Bpp, ImageFileFormat.Png);
        var optimized8Bpp = GetSavedSize(image8Bpp, ImageFileFormat.Png);
        var optimized1Bpp = GetSavedSize(image1Bpp, ImageFileFormat.Png);

        // All should be equal as since the logical pixel format is BW1, all should be converted to BW1 for saving.
        Assert.Equal(optimized24Bpp, optimized32Bpp);
        Assert.Equal(optimized24Bpp, optimized8Bpp);
        Assert.Equal(optimized24Bpp, optimized1Bpp);
    }

    // Gtk does not support saving with 1bpp/8bpp
    [PlatformFact(exclude: PlatformFlags.Linux)]
    public void SavePngWithUnoptimizedBitDepth()
    {
        var image32Bpp = LoadImage(ImageResources.dog_bw).CopyWithPixelFormat(ImagePixelFormat.ARGB32);
        var image24Bpp = LoadImage(ImageResources.dog_bw).CopyWithPixelFormat(ImagePixelFormat.RGB24);
        var image8Bpp = LoadImage(ImageResources.dog_bw).CopyWithPixelFormat(ImagePixelFormat.Gray8);
        var image1Bpp = LoadImage(ImageResources.dog_bw).CopyWithPixelFormat(ImagePixelFormat.BW1);

        // Specifying a PixelFormatHint equal to the real pixel format prevents optimized saving.

        var optimized32Bpp = GetSavedSize(image32Bpp, ImageFileFormat.Png);
        var unoptimized32Bpp = GetSavedSize(image32Bpp, ImageFileFormat.Png,
            new ImageSaveOptions { PixelFormatHint = ImagePixelFormat.ARGB32 });

        var optimized24Bpp = GetSavedSize(image24Bpp, ImageFileFormat.Png);
        var unoptimized24Bpp = GetSavedSize(image24Bpp, ImageFileFormat.Png,
            new ImageSaveOptions { PixelFormatHint = ImagePixelFormat.RGB24 });

        var optimized8Bpp = GetSavedSize(image8Bpp, ImageFileFormat.Png);
        var unoptimized8Bpp = GetSavedSize(image8Bpp, ImageFileFormat.Png,
            new ImageSaveOptions { PixelFormatHint = ImagePixelFormat.Gray8 });

        var optimized1Bpp = GetSavedSize(image1Bpp, ImageFileFormat.Png);
        var unoptimized1Bpp = GetSavedSize(image1Bpp, ImageFileFormat.Png,
            new ImageSaveOptions { PixelFormatHint = ImagePixelFormat.BW1 });

        // All optimized values should be less than their unoptimized counterparts.
        Assert.True(optimized32Bpp < unoptimized32Bpp);
        Assert.True(optimized24Bpp < unoptimized24Bpp);
        Assert.True(optimized8Bpp < unoptimized8Bpp);
        Assert.Equal(optimized1Bpp, unoptimized1Bpp);

        // Verify that 1bpp < 8bpp < 24bpp. 32bpp and 24bpp should be close but may vary so it isn't worth testing.
        Assert.True(unoptimized1Bpp < unoptimized8Bpp);
        Assert.True(unoptimized8Bpp < unoptimized24Bpp);
    }

    [Fact]
    public void PixelFormatHintDoesntLoseColor()
    {
        var original = LoadImage(ImageResources.dog);

        var stream = new MemoryStream();
        original.Save(stream, ImageFileFormat.Png, new ImageSaveOptions { PixelFormatHint = ImagePixelFormat.BW1 });

        var copy = ImageContext.Load(stream);
        ImageAsserts.Similar(ImageResources.dog, copy);
    }

    private int GetSavedSize(IMemoryImage image, ImageFileFormat fileFormat, ImageSaveOptions options = null)
    {
        var stream = new MemoryStream();
        image.Save(stream, fileFormat, options);
        return (int) stream.Length;
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
            ImageFileFormat.Png, ".png", "dog_gray_png",
            new[] { "dog_gray" }, new[] { ImagePixelFormat.Gray8 }, true
        },
        new object[]
        {
            ImageFileFormat.Png, ".png", "dog_gray_24bit_png",
            new[] { "dog_gray" }, new[] { ImagePixelFormat.Gray8 }, false
        },
        new object[]
        {
            ImageFileFormat.Png, ".png", "dog_bw",
            new[] { "dog_bw" }, new[] { ImagePixelFormat.BW1 }, false
        },
        new object[]
        {
            ImageFileFormat.Png, ".png", "dog_bw_24bit",
            new[] { "dog_bw" }, new[] { ImagePixelFormat.BW1 }, false
        },
        new object[]
        {
            ImageFileFormat.Jpeg, ".jpg", "dog",
            new[] { "dog" }, new[] { ImagePixelFormat.RGB24 }, false
        },
        new object[]
        {
            ImageFileFormat.Jpeg, ".jpg", "dog_gray",
            new[] { "dog_gray" }, new[] { ImagePixelFormat.Gray8 }, false
        },
        new object[]
        {
            ImageFileFormat.Jpeg, ".jpg", "dog_bw_jpg",
            new[] { "dog_bw_jpg" }, new[] { ImagePixelFormat.Gray8 }, false
        },
        new object[]
        {
            ImageFileFormat.Jpeg2000, ".jp2", "dog_jp2",
            new[] { "dog" }, new[] { ImagePixelFormat.RGB24 }, false
        },
        new object[]
        {
            ImageFileFormat.Bmp, ".bmp", "dog_bmp",
            new[] { "dog" }, new[] { ImagePixelFormat.RGB24 }, true
        },
        // TODO: Re-enable this for macOS - seems like a bug in 12.6, 12.5 was fine and apparently 13.0 works ok
#if !MAC
        new object[]
        {
            ImageFileFormat.Bmp, ".bmp", "dog_gray_bmp",
            new[] { "dog_gray" }, new[] { ImagePixelFormat.Gray8 }, true
        },
        new object[]
        {
            ImageFileFormat.Bmp, ".bmp", "dog_bw_bmp",
            new[] { "dog_bw" }, new[] { ImagePixelFormat.BW1 }, true
        },
        new object[]
        {
            ImageFileFormat.Bmp, ".bmp", "dog_bw_invertpal",
            new[] { "dog_bw" }, new[] { ImagePixelFormat.BW1 }, true
        },
#endif
        new object[]
        {
            ImageFileFormat.Tiff, ".tiff", "dog_alpha_tiff",
            new[] { "dog_alpha" }, new[] { ImagePixelFormat.ARGB32 }, false
        },
        new object[]
        {
            ImageFileFormat.Tiff, ".tiff", "dog_tiff",
            new[] { "dog" }, new[] { ImagePixelFormat.RGB24 }, false
        },
        new object[]
        {
            ImageFileFormat.Tiff, ".tiff", "dog_gray_tiff",
            new[] { "dog_gray" }, new[] { ImagePixelFormat.Gray8 }, true
        },
        new object[]
        {
            ImageFileFormat.Tiff, ".tiff", "dog_gray_24bit_tiff",
            new[] { "dog_gray" }, new[] { ImagePixelFormat.Gray8 }, false
        },
        new object[]
        {
            ImageFileFormat.Tiff, ".tiff", "dog_bw_tiff",
            new[] { "dog_bw" }, new[] { ImagePixelFormat.BW1 }, false
        },
        new object[]
        {
            ImageFileFormat.Tiff, ".tiff", "animals_tiff",
            new[] { "dog", "dog_h_p300", "stock_cat" },
            new[] { ImagePixelFormat.RGB24, ImagePixelFormat.RGB24, ImagePixelFormat.RGB24 }, false
        },
    };
}