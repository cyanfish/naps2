using NAPS2.Images.Bitwise;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class WhiteBlackPointOpTests : ContextualTests
{
    private const int W = 100;
    private const int H = 100;

    [Theory]
    [MemberData(nameof(ColorFormats))]
    public void NeutralPaper(ImagePixelFormat pixelFormat)
    {
        var image = CreatePage(pixelFormat, (230, 230, 230), (40, 40, 40));

        WhiteBlackPointOp.PerformFullOp(image, CorrectionMode.Document);

        Assert.Equal((255, 255, 255), GetPixel(image, 0, 0));
        Assert.Equal((0, 0, 0), GetPixel(image, W / 2, H / 2));
    }

    [Fact]
    public void SlightlyTintedPaper()
    {
        // Within MAX_PAPER_TINT, so it's flattened to white like neutral paper
        var image = CreatePage(ImagePixelFormat.RGB24, (232, 230, 236), (40, 40, 40));

        WhiteBlackPointOp.PerformFullOp(image, CorrectionMode.Document);

        var (r, g, b) = GetPixel(image, 0, 0);
        Assert.InRange(r, 250, 255);
        Assert.InRange(g, 250, 255);
        Assert.InRange(b, 250, 255);
        Assert.Equal((0, 0, 0), GetPixel(image, W / 2, H / 2));
    }

    [Fact]
    public void TintedPaper()
    {
        // The paper color is preserved, but the text is still darkened to black
        var image = CreatePage(ImagePixelFormat.RGB24, (240, 225, 195), (90, 85, 72));

        WhiteBlackPointOp.PerformFullOp(image, CorrectionMode.Document);

        Assert.Equal((240, 225, 195), GetPixel(image, 0, 0));
        var (r, g, b) = GetPixel(image, W / 2, H / 2);
        Assert.InRange(r, 0, 5);
        Assert.InRange(g, 0, 5);
        Assert.InRange(b, 0, 5);
    }

    [Fact]
    public void StronglyColoredPaper()
    {
        // The paper's blue channel is darker than the text's black point, so the image is left unchanged
        var image = CreatePage(ImagePixelFormat.RGB24, (240, 230, 70), (100, 100, 60));

        WhiteBlackPointOp.PerformFullOp(image, CorrectionMode.Document);

        Assert.Equal((240, 230, 70), GetPixel(image, 0, 0));
        Assert.Equal((100, 100, 60), GetPixel(image, W / 2, H / 2));
    }

    [Theory]
    [MemberData(nameof(ColorFormats))]
    public void BlankPage(ImagePixelFormat pixelFormat)
    {
        // A single peak can't be both white and black, so the image is left unchanged
        var image = CreatePage(pixelFormat, (230, 230, 230), null);

        WhiteBlackPointOp.PerformFullOp(image, CorrectionMode.Document);

        Assert.Equal((230, 230, 230), GetPixel(image, 0, 0));
        Assert.Equal((230, 230, 230), GetPixel(image, W / 2, H / 2));
    }

    private IMemoryImage CreatePage(ImagePixelFormat pixelFormat, (byte r, byte g, byte b) paper,
        (byte r, byte g, byte b)? text)
    {
        var image = ImageContext.Create(W, H, ImagePixelFormat.RGB24);
        new FillColorImageOp(paper.r, paper.g, paper.b, 255).Perform(image);
        if (text is var (tr, tg, tb))
        {
            // Text covers the middle 40% of the page
            using var rect = ImageContext.Create(W * 2 / 5, H, ImagePixelFormat.RGB24);
            new FillColorImageOp(tr, tg, tb, 255).Perform(rect);
            new CopyBitwiseImageOp { DestXOffset = W * 3 / 10 }.Perform(rect, image);
        }
        return pixelFormat == ImagePixelFormat.RGB24 ? image : image.CopyWithPixelFormat(pixelFormat);
    }

    private static unsafe (int r, int g, int b) GetPixel(IMemoryImage image, int x, int y)
    {
        using var lockState = image.Lock(LockMode.ReadOnly, out var data);
        var pixel = data.ptr + data.stride * y + data.bytesPerPixel * x;
        return (*(pixel + data.rOff), *(pixel + data.gOff), *(pixel + data.bOff));
    }

    public static IEnumerable<object[]> ColorFormats =
    [
        new object[] { ImagePixelFormat.ARGB32 },
        new object[] { ImagePixelFormat.RGB24 },
        new object[] { ImagePixelFormat.Gray8 }
    ];
}
