using NAPS2.Pdf;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

// The bundled encoders are process-wide, and some tests here temporarily replace the JPEG encoder, so don't run in
// parallel with other tests.
[CollectionDefinition(nameof(JpegliEncoderTests), DisableParallelization = true)]
public class JpegliEncoderTestsCollection;

[Collection(nameof(JpegliEncoderTests))]
public class JpegliEncoderTests : ContextualTests
{
    // Images with different color content (color, grayscale, black and white, transparency)
    public static IEnumerable<object[]> TestCases =
    [
        [nameof(ImageResources.dog)],
        [nameof(ImageResources.dog_gray)],
        [nameof(ImageResources.dog_bw)],
        [nameof(ImageResources.dog_alpha)]
    ];

    [Fact]
    public void RegisteredByScanningContext()
    {
        Assert.IsType<JpegliEncoder>(ImageContext.BundledEncoders[ImageFileFormat.Jpeg]);
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void SaveToStream(string resourceName)
    {
        using var image = LoadTestImage(resourceName);

        var stream = new MemoryStream();
        image.Save(stream, ImageFileFormat.Jpeg, new ImageSaveOptions { Quality = 90 });

        AssertJpegliOutput(image, stream.ToArray());
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void SaveToFile(string resourceName)
    {
        using var image = LoadTestImage(resourceName);
        var path = Path.Combine(FolderPath, "test.jpg");

        image.Save(path, options: new ImageSaveOptions { Quality = 90 });

        AssertJpegliOutput(image, File.ReadAllBytes(path));
    }

    [Fact]
    public void DefaultQualityIs75()
    {
        using var image = LoadImage(ImageResources.dog);

        var defaultQuality = image.SaveToMemoryStream(ImageFileFormat.Jpeg).ToArray();
        var quality75 = image.SaveToMemoryStream(ImageFileFormat.Jpeg, new ImageSaveOptions { Quality = 75 }).ToArray();

        Assert.Equal(quality75, defaultQuality);
    }

    [Fact]
    public void EncoderErrorThrows()
    {
        // JPEG doesn't support dimensions over 65500, so this fails inside jpegli
        using var image = ImageContext.Create(70000, 1, ImagePixelFormat.RGB24);
        var stream = new MemoryStream();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            ImageContext.BundledEncoders[ImageFileFormat.Jpeg].TryEncode(image, stream, new ImageSaveOptions()));

        Assert.Contains("Input image too big", ex.Message);
        Assert.Equal(0, stream.Length);
    }

    [Fact]
    public void FallbackToPlatformEncoderForStream()
    {
        var encoder = new FailingEncoder();
        var original = ImageContext.BundledEncoders[ImageFileFormat.Jpeg];
        ImageContext.BundledEncoders[ImageFileFormat.Jpeg] = encoder;
        try
        {
            using var image = LoadImage(ImageResources.dog);

            var stream = new MemoryStream();
            image.Save(stream, ImageFileFormat.Jpeg);

            Assert.True(encoder.Called);
            ImageAsserts.Similar(image, ImageContext.Load(stream.ToArray()), ImageAsserts.XL_RMSE_THRESHOLD);
        }
        finally
        {
            ImageContext.BundledEncoders[ImageFileFormat.Jpeg] = original;
        }
    }

    [Fact]
    public void FallbackToPlatformEncoderForFile()
    {
        var encoder = new FailingEncoder();
        var original = ImageContext.BundledEncoders[ImageFileFormat.Jpeg];
        ImageContext.BundledEncoders[ImageFileFormat.Jpeg] = encoder;
        try
        {
            using var image = LoadImage(ImageResources.dog);
            var path = Path.Combine(FolderPath, "test.jpg");

            image.Save(path);

            Assert.True(encoder.Called);
            ImageAsserts.Similar(image, ImageContext.Load(path), ImageAsserts.XL_RMSE_THRESHOLD);
        }
        finally
        {
            ImageContext.BundledEncoders[ImageFileFormat.Jpeg] = original;
        }
    }

    private IMemoryImage LoadTestImage(string resourceName)
    {
        var image = LoadImage((byte[]) ImageResources.ResourceManager.GetObject(resourceName)!);
        if (image.HorizontalResolution <= 0 || image.VerticalResolution <= 0)
        {
            image.SetResolution(150, 150);
        }
        return image;
    }

    private void AssertJpegliOutput(IMemoryImage original, byte[] jpeg)
    {
        // Saving should produce exactly the same output as using the encoder directly
        var directStream = new MemoryStream();
        Assert.True(ImageContext.BundledEncoders[ImageFileFormat.Jpeg]
            .TryEncode(original, directStream, new ImageSaveOptions { Quality = 90 }));
        Assert.Equal(directStream.ToArray(), jpeg);

        var header = JpegFormatHelper.ReadHeader(new MemoryStream(jpeg));
        Assert.NotNull(header);
        // jpegli output is always 3-channel with a JFIF header (and no EXIF header, unlike e.g. GDI)
        Assert.Equal(3, header.NumComponents);
        Assert.True(header.HasJfifHeader);
        Assert.False(header.HasExifHeader);
        Assert.Equal(original.Width, header.Width);
        Assert.Equal(original.Height, header.Height);
        Assert.Equal(Math.Round(original.HorizontalResolution), header.HorizontalDpi);
        Assert.Equal(Math.Round(original.VerticalResolution), header.VerticalDpi);

        // Compare with the RGB version of the image (as transparency etc. can't be represented in JPEG)
        using var expected = original.CopyWithPixelFormat(ImagePixelFormat.RGB24);
        using var actual = ImageContext.Load(jpeg);
        ImageAsserts.Similar(expected, actual);
    }

    private class FailingEncoder : IBundledEncoder
    {
        public bool Called { get; private set; }

        public bool TryEncode(IMemoryImage image, Stream stream, ImageSaveOptions options)
        {
            Called = true;
            return false;
        }
    }
}
