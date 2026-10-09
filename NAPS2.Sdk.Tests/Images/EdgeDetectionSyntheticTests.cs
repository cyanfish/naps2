using NAPS2.Images.Bitwise;
using Xunit;

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
        var image = CreateImage(pixelFormat, 0, image =>
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
        var image = CreateImage(pixelFormat, 255, _ => { });

        var op = Detect(image);

        AssertBounds(op, 0, 0, 0, 0);
    }

    [Theory]
    [MemberData(nameof(AllFormats))]
    public void IgnoresDust(ImagePixelFormat pixelFormat)
    {
        var image = CreateImage(pixelFormat, 0, image =>
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
        var image = CreateImage(pixelFormat, 0, image =>
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
        var image = CreateImage(pixelFormat, 0, image =>
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
        var image = CreateImage(ImagePixelFormat.RGB24, 230, image =>
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
        var image = CreateImage(ImagePixelFormat.RGB24, 238, image =>
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

    // Draws a page with some dark "text" lines on it
    private void DrawPage(IMemoryImage image, int x1, int y1, int x2, int y2, byte color)
    {
        FillRect(image, x1, y1, x2, y2, color);
        int marginX = (x2 - x1) / 8;
        int marginY = (y2 - y1) / 8;
        for (int y = y1 + marginY; y < y2 - marginY; y += 12)
        {
            FillRect(image, x1 + marginX, y, x2 - marginX, y + 4, 30);
        }
    }

    private void FillRect(IMemoryImage image, int x1, int y1, int x2, int y2, byte color)
    {
        using var rect = ImageContext.Create(x2 - x1, y2 - y1, ImagePixelFormat.RGB24);
        new FillColorImageOp(color, color, color, 255).Perform(rect);
        new CopyBitwiseImageOp { DestXOffset = x1, DestYOffset = y1 }.Perform(rect, image);
    }

    private IMemoryImage CreateImage(ImagePixelFormat pixelFormat, byte background, Action<IMemoryImage> draw)
    {
        var image = ImageContext.Create(W, H, ImagePixelFormat.RGB24);
        new FillColorImageOp(background, background, background, 255).Perform(image);
        draw(image);
        return pixelFormat == ImagePixelFormat.RGB24 ? image : image.CopyWithPixelFormat(pixelFormat);
    }

    public static IEnumerable<object[]> AllFormats =
    [
        new object[] { ImagePixelFormat.ARGB32 },
        new object[] { ImagePixelFormat.RGB24 },
        new object[] { ImagePixelFormat.Gray8 },
        new object[] { ImagePixelFormat.BW1 }
    ];
}
