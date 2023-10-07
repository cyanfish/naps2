using NAPS2.Pdf;
using Xunit;

namespace NAPS2.Sdk.Tests.Pdf;

public class JpegFormatHelperTests
{
    [Fact]
    public void Jfif()
    {
        var header = JpegFormatHelper.ReadHeader(new MemoryStream(ImageResources.dog));
        Assert.NotNull(header);
        Assert.Equal(788, header.Width);
        Assert.Equal(525, header.Height);
        Assert.Equal(3, header.NumComponents);
        Assert.Equal(72, header.HorizontalDpi);
        Assert.Equal(72, header.VerticalDpi);
        Assert.True(header.HasJfifHeader);
    }

    [Fact]
    public void JfifGrey()
    {
        var header = JpegFormatHelper.ReadHeader(new MemoryStream(ImageResources.dog_gray_8bit));
        Assert.NotNull(header);
        Assert.Equal(788, header.Width);
        Assert.Equal(525, header.Height);
        Assert.Equal(1, header.NumComponents);
        Assert.Equal(72, header.HorizontalDpi);
        Assert.Equal(72, header.VerticalDpi);
        Assert.True(header.HasJfifHeader);
    }

    [Fact]
    public void Exif()
    {
        var header = JpegFormatHelper.ReadHeader(new MemoryStream(ImageResources.dog_exif));
        Assert.NotNull(header);
        Assert.Equal(788, header.Width);
        Assert.Equal(525, header.Height);
        Assert.Equal(3, header.NumComponents);
        Assert.Equal(72, header.HorizontalDpi);
        Assert.Equal(72, header.VerticalDpi);
        Assert.False(header.HasJfifHeader);
        Assert.True(header.HasExifHeader);
    }
}