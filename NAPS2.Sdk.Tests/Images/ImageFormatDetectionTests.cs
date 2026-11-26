using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class ImageFormatDetectionTests
{
    [Theory]
    [InlineData("image.webp", ImageFileFormat.WebP)]
    [InlineData("image.jxl", ImageFileFormat.JpegXl)]
    [InlineData("image.heic", ImageFileFormat.Heic)]
    [InlineData("image.heif", ImageFileFormat.Heic)]
    [InlineData("image.avif", ImageFileFormat.Avif)]
    [InlineData("image.txt", ImageFileFormat.Unknown)]
    public void FileFormatFromExtension(string path, ImageFileFormat expected)
    {
        Assert.Equal(expected, ImageContext.GetFileFormatFromExtension(path));
    }

    [Theory]
    [InlineData(new byte[] { 0x52, 0x49, 0x46, 0x46, 1, 2, 3, 4, 0x57, 0x45, 0x42, 0x50 }, ImageFileFormat.WebP)]
    [InlineData(new byte[] { 0xFF, 0x0A, 1, 2 }, ImageFileFormat.JpegXl)]
    [InlineData(new byte[] { 0, 0, 0, 0x0C, 0x4A, 0x58, 0x4C, 0x20, 0x0D, 0x0A, 0x87, 0x0A }, ImageFileFormat.JpegXl)]
    // ftyp box with "heic"/"avif" as the major brand
    [InlineData(new byte[] { 0, 0, 0, 0x18, 0x66, 0x74, 0x79, 0x70, 0x68, 0x65, 0x69, 0x63, 0, 0, 0, 0 },
        ImageFileFormat.Heic)]
    [InlineData(new byte[] { 0, 0, 0, 0x18, 0x66, 0x74, 0x79, 0x70, 0x61, 0x76, 0x69, 0x66, 0, 0, 0, 0 },
        ImageFileFormat.Avif)]
    // "mif1" major brand (generic HEIF) with "avif" as a compatible brand
    [InlineData(new byte[]
    {
        0, 0, 0, 0x18, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x69, 0x66, 0x31, 0, 0, 0, 0, 0x6D, 0x69, 0x66, 0x31, 0x61, 0x76,
        0x69, 0x66
    }, ImageFileFormat.Avif)]
    // ftyp box for an unrelated format (MP4)
    [InlineData(new byte[] { 0, 0, 0, 0x18, 0x66, 0x74, 0x79, 0x70, 0x69, 0x73, 0x6F, 0x6D, 0, 0, 0, 0 },
        ImageFileFormat.Unknown)]
    public void FileFormatFromFirstBytes(byte[] firstBytes, ImageFileFormat expected)
    {
        Assert.Equal(expected, ImageContext.GetFileFormatFromFirstBytes(firstBytes));
    }
}
