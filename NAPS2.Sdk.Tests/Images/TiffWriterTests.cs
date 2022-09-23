using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class TiffWriterTests : ContextualTests
{
    // TODO: Add tests for error/edge cases (e.g. invalid files, mismatched extensions/format (?), tiff progress, saving 0/null images, unicode file names)
    // TODO: Verify rough expected file sizes to ensure compression is as expected (especially for tiff saving)

    private readonly ITiffWriter _tiffWriter;

    public TiffWriterTests()
    {
        _tiffWriter = ImageContext.TiffWriter;
    }

    [Fact]
    public void SaveSinglePageTiffToFile()
    {
        var path = Path.Combine(FolderPath, "image.tiff");
        var original = LoadImage(ImageResources.dog);

        _tiffWriter.SaveTiff(new[] { original }, path);
        AssertTiff(path, ImageResources.dog);
    }

    [Fact]
    public void SaveMultiPageTiffToFile()
    {
        var path = Path.Combine(FolderPath, "image.tiff");
        var original = new[]
        {
            LoadImage(ImageResources.dog),
            LoadImage(ImageResources.dog_bw),
            LoadImage(ImageResources.stock_cat)
        };

        _tiffWriter.SaveTiff(original, path);
        AssertTiff(path, ImageResources.dog, ImageResources.dog_bw, ImageResources.stock_cat);
    }

    [Fact]
    public void SaveSinglePageTiffToStream()
    {
        var stream = new MemoryStream();
        var original = LoadImage(ImageResources.dog);

        _tiffWriter.SaveTiff(new[] { original }, stream);
        AssertTiff(stream, ImageResources.dog);
    }

    [Fact]
    public void SaveMultiPageTiffToStream()
    {
        var stream = new MemoryStream();
        var original = new[]
        {
            LoadImage(ImageResources.dog),
            LoadImage(ImageResources.dog_bw),
            LoadImage(ImageResources.stock_cat)
        };

        _tiffWriter.SaveTiff(original, stream);
        AssertTiff(stream, ImageResources.dog, ImageResources.dog_bw, ImageResources.stock_cat);
    }

    [Fact]
    public void SaveBlackAndWhiteTiff()
    {
        var path = Path.Combine(FolderPath, "image.tiff");
        var original = LoadImage(ImageResources.dog_bw);

        _tiffWriter.SaveTiff(new[] { original }, path);
        AssertTiff(path, ImageResources.dog_bw);
    }

    [Fact]
    public void SaveColorTiffWithG4()
    {
        var path = Path.Combine(FolderPath, "image.tiff");
        var original = LoadImage(ImageResources.dog_png);

        _tiffWriter.SaveTiff(new[] { original }, path, TiffCompressionType.Ccitt4);
        AssertTiff(path, ImageResources.dog_bw);
    }

    private void AssertTiff(string path, params byte[][] expectedImages)
    {
        var actual = ImageContext.LoadFrames(path, out var count).ToArray();
        DoAssertTiff(actual, count, expectedImages);
    }

    private void AssertTiff(Stream stream, params byte[][] expectedImages)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var actual = ImageContext.LoadFrames(stream, out var count).ToArray();
        DoAssertTiff(actual, count, expectedImages);
    }

    private static void DoAssertTiff(IMemoryImage[] actual, int count, byte[][] expectedImages)
    {
        Assert.Equal(expectedImages.Length, count);
        Assert.Equal(expectedImages.Length, actual.Length);
        for (int i = 0; i < expectedImages.Length; i++)
        {
            Assert.Equal(ImageFileFormat.Tiff, actual[i].OriginalFileFormat);
            ImageAsserts.Similar(expectedImages[i], actual[i]);
        }
    }
}