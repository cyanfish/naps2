using NAPS2.Images.Bitwise;
using Xunit;
using static NAPS2.Sdk.Tests.Images.SyntheticImageGenerator;

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

        Assert.Equal(WHITE, GetPixel(image, 0, 0));
        Assert.Equal(BLACK, GetPixel(image, W / 2, H / 2));
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
        Assert.Equal(BLACK, GetPixel(image, W / 2, H / 2));
    }

    [Fact]
    public void TintedPaper()
    {
        // The paper color is preserved, but the text is still darkened to black
        var paper = (240, 225, 195);
        var image = CreatePage(ImagePixelFormat.RGB24, paper, (90, 85, 72));

        WhiteBlackPointOp.PerformFullOp(image, CorrectionMode.Document);

        Assert.Equal(paper, GetPixel(image, 0, 0));
        var (r, g, b) = GetPixel(image, W / 2, H / 2);
        Assert.InRange(r, 0, 5);
        Assert.InRange(g, 0, 5);
        Assert.InRange(b, 0, 5);
    }

    [Fact]
    public void StronglyColoredPaper()
    {
        // The paper's blue channel is darker than the text's black point, so the image is left unchanged
        var paper = (240, 230, 70);
        var text = (100, 100, 60);
        var image = CreatePage(ImagePixelFormat.RGB24, paper, text);

        WhiteBlackPointOp.PerformFullOp(image, CorrectionMode.Document);

        Assert.Equal(paper, GetPixel(image, 0, 0));
        Assert.Equal(text, GetPixel(image, W / 2, H / 2));
    }

    [Theory]
    [MemberData(nameof(ColorFormats))]
    public void BlankPage(ImagePixelFormat pixelFormat)
    {
        // A single peak can't be both white and black, so the image is left unchanged
        var paper = (230, 230, 230);
        var image = CreatePage(pixelFormat, paper, null);

        WhiteBlackPointOp.PerformFullOp(image, CorrectionMode.Document);

        Assert.Equal(paper, GetPixel(image, 0, 0));
        Assert.Equal(paper, GetPixel(image, W / 2, H / 2));
    }

    private IMemoryImage CreatePage(ImagePixelFormat pixelFormat, Rgb paper,
        Rgb? text) =>
        // Text covers the middle 40% of the page
        CreateSyntheticImage(ImageContext, pixelFormat, W, H, paper,
            text is { } textColor
                ? image => FillRect(image, W * 3 / 10, 0, W * 7 / 10, H, textColor)
                : null);

    private static unsafe Rgb GetPixel(IMemoryImage image, int x, int y)
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
