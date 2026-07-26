using NAPS2.Images.Bitwise;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class AutoCropTests : ContextualTests
{
    /// <summary>
    /// Creates a white image with a single black rectangle of content.
    /// </summary>
    private IMemoryImage CreateImageWithContent(int width, int height,
        int contentX, int contentY, int contentW, int contentH)
    {
        var image = ImageContext.Create(width, height, ImagePixelFormat.RGB24);
        FillColorImageOp.White.Perform(image);
        using var content = ImageContext.Create(contentW, contentH, ImagePixelFormat.RGB24);
        FillColorImageOp.Black.Perform(content);
        new CopyBitwiseImageOp { DestXOffset = contentX, DestYOffset = contentY }.Perform(content, image);
        return image;
    }

    private static AutoCropSettings ExactSettings(AutoCropAxisMode width, AutoCropAxisMode height,
        int? fixedWidthPx = null, int? fixedHeightPx = null) => new()
    {
        WidthMode = width,
        HeightMode = height,
        FixedWidthPx = fixedWidthPx,
        FixedHeightPx = fixedHeightPx,
        PaddingPx = 0,
        MinContentPixels = 1,
        ThresholdFraction = 0
    };

    [Fact]
    public void AutoBothAxes()
    {
        using var image = CreateImageWithContent(200, 400, 50, 40, 100, 120);
        var t = AutoCropper.GetCropTransform(image, ExactSettings(AutoCropAxisMode.Auto, AutoCropAxisMode.Auto));
        Assert.NotNull(t);
        Assert.Equal(50, t!.Left);
        Assert.Equal(50, t.Right); // 200 - 1 - 149
        Assert.Equal(40, t.Top);
        Assert.Equal(240, t.Bottom); // 400 - 1 - 159
        using var cropped = image.PerformTransform(t);
        Assert.Equal(100, cropped.Width);
        Assert.Equal(120, cropped.Height);
    }

    [Fact]
    public void AutoHeightTrimsOverLongTail()
    {
        // Simulates a receipt scanned with an over-long scan area: content only near the
        // top, the rest blank. Height should be trimmed back to the content.
        using var image = CreateImageWithContent(200, 1000, 20, 20, 160, 200);
        var t = AutoCropper.GetCropTransform(image, ExactSettings(AutoCropAxisMode.Off, AutoCropAxisMode.Auto));
        Assert.NotNull(t);
        Assert.Equal(0, t!.Left);
        Assert.Equal(0, t.Right);
        Assert.Equal(20, t.Top);
        Assert.Equal(780, t.Bottom); // 1000 - 1 - 219
        using var cropped = image.PerformTransform(t);
        Assert.Equal(200, cropped.Width);
        Assert.Equal(200, cropped.Height);
    }

    [Fact]
    public void FixedWidthAutoHeight()
    {
        // The receipt use case: fixed width, auto-detected height.
        using var image = CreateImageWithContent(200, 400, 50, 40, 100, 120);
        var t = AutoCropper.GetCropTransform(image,
            ExactSettings(AutoCropAxisMode.Fixed, AutoCropAxisMode.Auto, fixedWidthPx: 80));
        Assert.NotNull(t);
        using var cropped = image.PerformTransform(t!);
        Assert.Equal(80, cropped.Width); // forced to the fixed width
        Assert.Equal(120, cropped.Height); // auto-detected
    }

    [Fact]
    public void FixedWidthIsCenteredOnContent()
    {
        using var image = CreateImageWithContent(200, 400, 50, 40, 100, 120);
        var t = AutoCropper.GetCropTransform(image,
            ExactSettings(AutoCropAxisMode.Fixed, AutoCropAxisMode.Off, fixedWidthPx: 80));
        Assert.NotNull(t);
        // Content centre x = (50 + 149) / 2 = 99; window = 99 - 40 = 59.
        Assert.Equal(59, t!.Left);
        Assert.Equal(61, t.Right); // 200 - (59 + 80)
    }

    [Fact]
    public void BlankPageIsNotCropped()
    {
        using var image = ImageContext.Create(200, 400, ImagePixelFormat.RGB24);
        FillColorImageOp.White.Perform(image);
        var t = AutoCropper.GetCropTransform(image, ExactSettings(AutoCropAxisMode.Auto, AutoCropAxisMode.Auto));
        Assert.Null(t);
    }

    [Fact]
    public void OffOffReturnsNull()
    {
        using var image = CreateImageWithContent(200, 400, 50, 40, 100, 120);
        var t = AutoCropper.GetCropTransform(image, ExactSettings(AutoCropAxisMode.Off, AutoCropAxisMode.Off));
        Assert.Null(t);
    }

    [Fact]
    public void PaddingExpandsContentBox()
    {
        using var image = CreateImageWithContent(200, 400, 50, 40, 100, 120);
        var settings = new AutoCropSettings
        {
            WidthMode = AutoCropAxisMode.Auto,
            HeightMode = AutoCropAxisMode.Auto,
            PaddingPx = 10,
            MinContentPixels = 1,
            ThresholdFraction = 0
        };
        var t = AutoCropper.GetCropTransform(image, settings);
        Assert.NotNull(t);
        Assert.Equal(40, t!.Left); // 50 - 10
        Assert.Equal(40, t.Right); // 50 - 10
        Assert.Equal(30, t.Top); // 40 - 10
        Assert.Equal(230, t.Bottom); // 240 - 10
    }
}
