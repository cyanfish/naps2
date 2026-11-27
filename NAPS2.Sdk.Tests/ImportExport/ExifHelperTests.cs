using System.Buffers.Binary;
using NAPS2.ImportExport;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport;

public class ExifHelperTests
{
    // Offsets in the output of CreateResolutionExif: the IFD (entry count, then XResolution, YResolution and
    // ResolutionUnit entries), then the XResolution and YResolution rationals
    private const int ENTRY_COUNT = 8;
    private const int X_RES_TYPE = 12;
    private const int UNIT_VALUE = 42;
    private const int X_RES_VALUE = 50;

    [Theory]
    [InlineData(300, 300)]
    [InlineData(150.5, 72)]
    [InlineData(1200, 0.125)]
    public void RoundTrip(double xDpi, double yDpi)
    {
        var exif = ExifHelper.CreateResolutionExif(xDpi, yDpi);

        Assert.True(ExifHelper.IsExif(exif));
        Assert.Equal((xDpi, yDpi), ExifHelper.ReadResolution(exif));
        // Also with the marker used in JPEG and WebP files
        Assert.Equal((xDpi, yDpi), ExifHelper.ReadResolution([.."Exif\0\0"u8.ToArray(), ..exif]));
    }

    [Fact]
    public void InvalidResolutionIsNotWritten()
    {
        Assert.Null(ExifHelper.ReadResolution(ExifHelper.CreateResolutionExif(double.NaN, -1)));
    }

    [Fact]
    public void Centimeters()
    {
        var exif = ExifHelper.CreateResolutionExif(100, 100);
        exif[UNIT_VALUE] = 3;

        Assert.Equal((254, 254), ExifHelper.ReadResolution(exif));
    }

    [Fact]
    public void UnsignedRational()
    {
        // 3000000000 / 10000000 (above int.MaxValue, so it'd be negative if read as signed)
        var exif = ExifHelper.CreateResolutionExif(300, 300);
        BinaryPrimitives.WriteUInt32LittleEndian(exif.AsSpan(X_RES_VALUE), 3_000_000_000);
        BinaryPrimitives.WriteUInt32LittleEndian(exif.AsSpan(X_RES_VALUE + 4), 10_000_000);

        Assert.Equal((300, 300), ExifHelper.ReadResolution(exif));
    }

    [Fact]
    public void IgnoresWrongType()
    {
        var exif = ExifHelper.CreateResolutionExif(300, 300);
        exif[X_RES_TYPE] = 3; // Short instead of rational

        Assert.Null(ExifHelper.ReadResolution(exif));
    }

    [Fact]
    public void IgnoresEntriesBeyondData()
    {
        var exif = ExifHelper.CreateResolutionExif(300, 200);
        BinaryPrimitives.WriteUInt16LittleEndian(exif.AsSpan(ENTRY_COUNT), 60000);

        Assert.Equal((300, 200), ExifHelper.ReadResolution(exif));
    }

    [Fact]
    public void Malformed()
    {
        Assert.False(ExifHelper.IsExif("II*"u8.ToArray()));
        Assert.False(ExifHelper.IsExif(ExifHelper.CreateResolutionExif(300, 300), -1));
        // IFD offset outside the data (big-endian)
        Assert.Null(ExifHelper.ReadResolution([0x4D, 0x4D, 0x00, 0x2A, 0x7F, 0xFF, 0xFF, 0xFF]));
        // Value offset outside the data
        var exif = ExifHelper.CreateResolutionExif(300, 300);
        BinaryPrimitives.WriteInt32LittleEndian(exif.AsSpan(ENTRY_COUNT + 2 + 8), 100000);
        Assert.Null(ExifHelper.ReadResolution(exif));
    }
}
