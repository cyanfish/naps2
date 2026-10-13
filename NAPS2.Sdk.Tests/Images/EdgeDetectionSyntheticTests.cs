using NAPS2.Images.Bitwise;
using Xunit;
using static NAPS2.Sdk.Tests.Images.SyntheticImageGenerator;

namespace NAPS2.Sdk.Tests.Images;

public class EdgeDetectionSyntheticTests : ContextualTests
{
    private const int W = 400;
    private const int H = 500;
    private const int TOLERANCE = 2;

    [Theory]
    [MemberData(nameof(AllFormats))]
    public void PageOnBlackBackground(ImagePixelFormat pixelFormat)
    {
        var image = CreateSyntheticImage(ImageContext, pixelFormat, W, H, 0, image =>
        {
            DrawPage(image, 40, 50, 340, 470, 255);
        });

        var op = Detect(image);

        AssertBounds(op, 40, 50, W - 340, H - 470);
    }

    [Theory]
    [MemberData(nameof(AllFormats))]
    public void BlankImage(ImagePixelFormat pixelFormat)
    {
        var image = CreateSyntheticImage(ImageContext, pixelFormat, W, H, 255);

        var op = Detect(image);

        AssertBounds(op, 0, 0, 0, 0);
    }

    [Theory]
    [MemberData(nameof(AllFormats))]
    public void IgnoresDust(ImagePixelFormat pixelFormat)
    {
        var image = CreateSyntheticImage(ImageContext, pixelFormat, W, H, 0, image =>
        {
            DrawPage(image, 40, 50, 340, 470, 255);
            FillRect(image, 10, 10, 13, 13, 255);
            FillRect(image, 20, 200, 22, 203, 255);
            FillRect(image, 370, 480, 373, 482, 255);
        });

        var op = Detect(image);

        AssertBounds(op, 40, 50, W - 340, H - 470);
    }

    [Theory]
    [MemberData(nameof(AllFormats))]
    public void IgnoresScannerEdgeArtifacts(ImagePixelFormat pixelFormat)
    {
        var image = CreateSyntheticImage(ImageContext, pixelFormat, W, H, 0, image =>
        {
            DrawPage(image, 40, 50, 340, 470, 255);
            FillRect(image, 0, 0, 1, H, 255);
            FillRect(image, 0, H - 1, W, H, 255);
        });

        var op = Detect(image);

        AssertBounds(op, 40, 50, W - 340, H - 470);
    }

    [Theory]
    [MemberData(nameof(AllFormats))]
    public void TooSmallPage(ImagePixelFormat pixelFormat)
    {
        var image = CreateSyntheticImage(ImageContext, pixelFormat, W, H, 0, image =>
        {
            FillRect(image, 150, 150, 180, 400, 255);
        });

        var op = Detect(image);

        AssertBounds(op, 0, 0, 0, 0);
    }

    [Fact]
    public void ItemWithHalo()
    {
        // A dark item (e.g. a card) in the corner of a light background, with a soft bright halo around it
        const int itemW = 160, itemH = 100, halo = 8;
        var image = CreateSyntheticImage(ImageContext, ImagePixelFormat.RGB24, W, H, 230, image =>
        {
            for (int k = halo - 1; k >= 0; k--)
            {
                byte color = (byte) (244 - k * 14 / halo);
                FillRect(image, 0, 0, itemW + k + 1, itemH + k + 1, color);
            }
            DrawPage(image, 0, 0, itemW, itemH, 100);
        });

        var op = Detect(image);

        // The crop should be at the item, not the outside of the halo
        AssertBounds(op, 0, 0, W - itemW, H - itemH);
    }

    [Fact]
    public void ItemInMiddleWithScannerBorder()
    {
        // An item in the middle of the scan area, with a dark border along the left side of the scan that gradually
        // fades out (further than the ignored area at the edge of the image)
        byte[] border = [60, 225, 230, 233, 235, 237];
        var image = CreateSyntheticImage(ImageContext, ImagePixelFormat.RGB24, W, H, 238, image =>
        {
            for (int x = 0; x < border.Length; x++)
            {
                FillRect(image, x, 0, x + 1, H, border[x]);
            }
            DrawPage(image, 150, 200, 300, 350, 100);
        });

        var op = Detect(image);

        AssertBounds(op, 150, 200, W - 300, H - 350);
    }

    [Fact]
    public void PageOnStripedDarkBackground()
    {
        // Noise on a black background can look like faint lines, which is different than white backgrounds
        const int pageW = 150, pageH = 300;
        var random = new Random(0);
        var image = CreateSyntheticImage(ImageContext, ImagePixelFormat.RGB24, W, H, 35, image =>
        {
            for (int x = 0; x < W; x++)
            {
                FillRect(image, x, 0, x + 1, H, 35 + random.Next(-6, 7));
            }
            DrawPage(image, 0, 0, pageW, pageH, 245);
        });

        var op = Detect(image);

        AssertBounds(op, 0, 0, W - pageW, H - pageH);
    }

    [Theory]
    [InlineData(255, false)]
    [InlineData(255, true)]
    [InlineData(190, false)]
    [InlineData(190, true)]
    public void PageOnDarkBackgroundWithPadding(int fill, bool jpeg)
    {
        // The scan area only goes down to the bottom of the page (and not all the way to the right), and the rest of
        // the image is padded with a single color. From the top, white padding looks like a page on the dark
        // background. With JPEG compression, the padding isn't exactly one color near the scan area.
        const int pageW = 150, pageH = 300, scanW = 320;
        var image = CreateSyntheticImage(ImageContext, ImagePixelFormat.RGB24, W, H, 35, image =>
        {
            DrawPage(image, 0, 0, pageW, pageH, 245);
            FillRect(image, scanW, 0, W, H, fill);
            FillRect(image, 0, pageH, W, H, fill);
        });
        if (jpeg)
        {
            image = ImageContext.Load(image.SaveToMemoryStream(ImageFileFormat.Jpeg,
                new ImageSaveOptions { Quality = 75 }));
        }

        var op = Detect(image);

        AssertBounds(op, 0, 0, W - pageW, H - pageH);
    }

    [Theory]
    [MemberData(nameof(AllFormats))]
    public void PaddedOnLeftAndTop(ImagePixelFormat pixelFormat)
    {
        // White padding on the left and top, where the scan area is smaller than the image. The left padding isn't a
        // multiple of 8 so that black & white images are read across byte boundaries.
        var image = CreateSyntheticImage(ImageContext, pixelFormat, W, H, 0, image =>
        {
            FillRect(image, 0, 0, 13, H, 255);
            FillRect(image, 0, 0, W, 9, 255);
            DrawPage(image, 60, 70, 300, 400, 255);
        });

        var op = Detect(image);

        AssertBounds(op, 60, 70, W - 300, H - 400);
    }

    [Theory]
    [MemberData(nameof(AllFormats))]
    public void SaturatedPage(ImagePixelFormat pixelFormat)
    {
        // A page that fills the scan, where the paper is pure white. The blank margins aren't padding. The text is drawn
        // as words with gaps, as solid lines would look like a dark scan area padded with white.
        var image = CreateSyntheticImage(ImageContext, pixelFormat, W, H, 255, image =>
        {
            for (int y = 60; y < H - 60; y += 12)
            {
                for (int x = 50; x < W - 50; x += 30)
                {
                    FillRect(image, x, y, x + 20, y + 4, 30);
                }
            }
        });

        var op = Detect(image);

        AssertBounds(op, 0, 0, 0, 0);
    }

    private static EdgeDetectionImageOp Detect(IMemoryImage image)
    {
        var op = new EdgeDetectionImageOp();
        op.Perform(image);
        return op;
    }

    private static void AssertBounds(EdgeDetectionImageOp op, int left, int top, int right, int bottom)
    {
        Assert.InRange(op.Left, left - TOLERANCE, left + TOLERANCE);
        Assert.InRange(op.Top, top - TOLERANCE, top + TOLERANCE);
        Assert.InRange(op.Right, right - TOLERANCE, right + TOLERANCE);
        Assert.InRange(op.Bottom, bottom - TOLERANCE, bottom + TOLERANCE);
    }

    public static IEnumerable<object[]> AllFormats =
    [
        new object[] { ImagePixelFormat.ARGB32 },
        new object[] { ImagePixelFormat.RGB24 },
        new object[] { ImagePixelFormat.Gray8 },
        new object[] { ImagePixelFormat.BW1 }
    ];
}
