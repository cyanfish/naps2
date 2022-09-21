using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class TransformTests : ContextualTests
{
    // TODO: Test handling of other pixel formats
    // ARGB32 -> should work (ignoring alpha channel)
    // Gray -> commutativity tests mostly done, need to verify ones that don't work
    // BW1 -> should work where applicable
    // Unsupported -> should throw an exception
    // This might require some actual changes to the transforms.

    [Fact]
    public void BrightnessNull()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog);

        actual = actual.PerformTransform(new BrightnessTransform());

        ImageAsserts.Similar(expected, actual, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void BrightnessP300()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_b_p300);

        actual = actual.PerformTransform(new BrightnessTransform(300));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void BrightnessN300()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_b_n300);

        actual = actual.PerformTransform(new BrightnessTransform(-300));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void ContrastNull()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog);

        actual = actual.PerformTransform(new TrueContrastTransform());

        ImageAsserts.Similar(expected, actual, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void ContrastP300()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_c_p300);

        actual = actual.PerformTransform(new TrueContrastTransform(300));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void ContrastN300()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_c_n300);

        actual = actual.PerformTransform(new TrueContrastTransform(-300));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void HueNull()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog);

        actual = actual.PerformTransform(new HueTransform());

        ImageAsserts.Similar(expected, actual, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void HueP300()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_h_p300);

        actual = actual.PerformTransform(new HueTransform(300));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void HueN300()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_h_n300);

        actual = actual.PerformTransform(new HueTransform(-300));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SaturationNull()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog);

        actual = actual.PerformTransform(new SaturationTransform());

        ImageAsserts.Similar(expected, actual, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SaturationP300()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_s_p300);

        actual = actual.PerformTransform(new SaturationTransform(300));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SaturationN300()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_s_n300);

        actual = actual.PerformTransform(new SaturationTransform(-300));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SharpenNull()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog);

        actual = actual.PerformTransform(new SharpenTransform());

        ImageAsserts.Similar(expected, actual, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SharpenP300()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_sh_p1000);

        actual = actual.PerformTransform(new SharpenTransform(1000));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SharpenN300()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_sh_n1000);

        actual = actual.PerformTransform(new SharpenTransform(-1000));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void RotationNull()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog);

        actual = actual.PerformTransform(new RotationTransform());

        ImageAsserts.Similar(expected, actual, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void RotationP90()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_r_p90);

        actual = actual.PerformTransform(new RotationTransform(90));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void RotationP46()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_r_p46);

        actual = actual.PerformTransform(new RotationTransform(46));

        ImageAsserts.Similar(expected, actual, ImageAsserts.XPLAT_RMSE_THRESHOLD);
    }

    [Fact]
    public void RotationN45()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_r_n45);

        actual = actual.PerformTransform(new RotationTransform(-45));

        // TODO: The mac rotated image looks way better than gdi, consider if we can improve the gdi end
        ImageAsserts.Similar(expected, actual, ImageAsserts.XPLAT_RMSE_THRESHOLD);
    }

    [Fact]
    public void Rotation180()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage actual2 = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_r_180);

        actual = actual.PerformTransform(new RotationTransform(180));
        actual2 = actual2.PerformTransform(new RotationTransform(-180));

        ImageAsserts.Similar(actual2, actual, 0);
        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    // TODO: Add tests for rotating black and white images

    [Fact]
    public void ScaleNull()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog);

        actual = actual.PerformTransform(new ScaleTransform());

        ImageAsserts.Similar(expected, actual, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void Scale50Percent()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_sc_50pct);

        actual = actual.PerformTransform(new ScaleTransform(0.5));

        ImageAsserts.Similar(expected, actual, ImageAsserts.XPLAT_RMSE_THRESHOLD, ignoreResolution: true);
    }

    [Theory]
    [MemberData(nameof(CommutativeGrayTransforms))]
    public void GrayTransformsAreCommutative(Transform transform)
    {
        IMemoryImage original = LoadImage(ImageResources.dog);

        var actual = original.CopyWithPixelFormat(ImagePixelFormat.Gray8);
        actual = actual.PerformTransform(transform);

        var expected = original.Clone();
        expected = expected.PerformTransform(transform);
        expected = expected.CopyWithPixelFormat(ImagePixelFormat.Gray8);

        ImageAsserts.Similar(expected, actual);
    }

    [Fact]
    public void Scale1000Percent()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_huge);

        actual = actual.PerformTransform(new ScaleTransform(10));

        ImageAsserts.Similar(expected, actual, ImageAsserts.XPLAT_RMSE_THRESHOLD, ignoreResolution: true);
    }

    [Fact]
    public void CropNull()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog);

        actual = actual.PerformTransform(new CropTransform());

        ImageAsserts.Similar(expected, actual, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void Crop()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_c_5_10_15_20);

        actual = actual.PerformTransform(new CropTransform(10, 20, 15, 5));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void CropBlackWhiteBeforeAfter()
    {
        IMemoryImage first = LoadImage(ImageResources.dog);
        IMemoryImage second = LoadImage(ImageResources.dog);

        first = first.PerformTransform(new BlackWhiteTransform());
        first = first.PerformTransform(new CropTransform(10, 20, 15, 5));
        second = second.PerformTransform(new CropTransform(10, 20, 15, 5));
        second = second.PerformTransform(new BlackWhiteTransform());

        ImageAsserts.Similar(first, second, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void CropWithOriginal()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_c_5_10_15_20);

        actual = actual.PerformTransform(new CropTransform(10, 20, 15, 5, actual.Width, actual.Height));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void CropWithDifferentOriginal()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_c_5_10_15_20);

        actual = ImageContext.PerformTransform(actual,
            new CropTransform(20, 40, 30, 10, actual.Width * 2, actual.Height * 2));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void CropOutOfBounds()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog);

        actual = actual.PerformTransform(new CropTransform(-1, -1, -1, -1));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void BlackWhite()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_bw);

        actual = actual.PerformTransform(new BlackWhiteTransform());
        Assert.Equal(ImagePixelFormat.BW1, actual.LogicalPixelFormat);

        // TODO: There's no inherent reason this shouldn't be an exact match, unless I guess if
        // there's a slight pixel difference between png loading on mac/gdi
        ImageAsserts.Similar(expected, actual, ImageAsserts.XPLAT_RMSE_THRESHOLD);
    }

    [Fact]
    public void BlackWhiteP300()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_bw_p300);

        actual = actual.PerformTransform(new BlackWhiteTransform(300));
        Assert.Equal(ImagePixelFormat.BW1, actual.LogicalPixelFormat);

        ImageAsserts.Similar(expected, actual, ImageAsserts.XPLAT_RMSE_THRESHOLD);
    }

    [Fact]
    public void ColorBitDepth()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog_bw);
        IMemoryImage expected = LoadImage(ImageResources.dog_bw_24bit);

        actual = actual.PerformTransform(new ColorBitDepthTransform());
        Assert.Equal(ImagePixelFormat.RGB24, actual.PixelFormat);

        ImageAsserts.Similar(expected, actual, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void Thumbnail()
    {
        IMemoryImage actual = LoadImage(ImageResources.dog);
        IMemoryImage expected = LoadImage(ImageResources.dog_thumb_256);

        actual = actual.PerformTransform(new ThumbnailTransform(256));

#if NET6_0_OR_GREATER
        Assert.Equal(expected.Width, actual.Width);
#else
        // TODO: The image is off by 1 pixel, it might have something to do with how the cggraphics coordinate
        // system is a double and not pixel-aligned
        // TODO: Fix expected resolution
        ImageAsserts.Similar(expected, actual, ImageAsserts.XPLAT_RMSE_THRESHOLD, ignoreResolution: true);
#endif
    }

    public static IEnumerable<object[]> CommutativeGrayTransforms = new List<object[]>
    {
        // TODO: These don't work - should they?
        // new object[] { new BrightnessTransform(300) },
        // new object[] { new TrueContrastTransform(300) },
        // new object[] { new HueTransform(300) },
        // new object[] { new SaturationTransform(300) },
        new object[] { new ScaleTransform(0.5) },
        new object[] { new RotationTransform(46) },
        new object[] { new ThumbnailTransform() },
        new object[] { new CropTransform(10, 10, 10, 10) }
    };
}