using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class TransformTests : ContextualTests
{
    // TODO: Test handling of other pixel formats
    // ARGB32 -> should work (ignoring alpha channel)
    // Gray -> done
    // BW1 -> should work where applicable
    // Unsupported -> should throw an exception
    // This might require some actual changes to the transforms.

    [Fact]
    public void BrightnessNull()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog);

        var transformed = original.PerformTransform(new BrightnessTransform());

        ImageAsserts.Similar(expected, transformed, ImageAsserts.NULL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void BrightnessP300()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_b_p300);

        var transformed = original.PerformTransform(new BrightnessTransform(300));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void BrightnessN300()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_b_n300);

        var transformed = original.PerformTransform(new BrightnessTransform(-300));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void ContrastNull()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog);

        var transformed = original.PerformTransform(new TrueContrastTransform());

        ImageAsserts.Similar(expected, transformed, ImageAsserts.NULL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void ContrastP300()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_c_p300);

        var transformed = original.PerformTransform(new TrueContrastTransform(300));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void ContrastN300()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_c_n300);

        var transformed = original.PerformTransform(new TrueContrastTransform(-300));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void HueNull()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog);

        var transformed = original.PerformTransform(new HueTransform());

        ImageAsserts.Similar(expected, transformed, ImageAsserts.NULL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void HueP300()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_h_p300);

        var transformed = original.PerformTransform(new HueTransform(300));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void HueN300()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_h_n300);

        var transformed = original.PerformTransform(new HueTransform(-300));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void SaturationNull()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog);

        var transformed = original.PerformTransform(new SaturationTransform());

        ImageAsserts.Similar(expected, transformed, ImageAsserts.NULL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void SaturationP300()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_s_p300);

        var transformed = original.PerformTransform(new SaturationTransform(300));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void SaturationN300()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_s_n300);

        var transformed = original.PerformTransform(new SaturationTransform(-300));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void SharpenNull()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog);

        var transformed = original.PerformTransform(new SharpenTransform());

        ImageAsserts.Similar(expected, transformed, ImageAsserts.NULL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void SharpenP300()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_sh_p1000);

        var transformed = original.PerformTransform(new SharpenTransform(1000));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void SharpenN300()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_sh_n1000);

        var transformed = original.PerformTransform(new SharpenTransform(-1000));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void RotationNull()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog);

        var transformed = original.PerformTransform(new RotationTransform());

        ImageAsserts.Similar(expected, transformed, ImageAsserts.NULL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void RotationP90()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_r_p90);

        var transformed = original.PerformTransform(new RotationTransform(90));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void RotationP46()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_r_p46);

        var transformed = original.PerformTransform(new RotationTransform(46));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.XPLAT_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void RotationN45()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_r_n45);

        var transformed = original.PerformTransform(new RotationTransform(-45));

        // TODO: The mac rotated image looks way better than gdi, consider if we can improve the gdi end
        ImageAsserts.Similar(expected, transformed, ImageAsserts.XPLAT_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void Rotation180()
    {
        var original = LoadImage(ImageResources.dog);
        var transformed2 = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_r_180);

        var transformed = original.PerformTransform(new RotationTransform(180));
        transformed2 = transformed2.PerformTransform(new RotationTransform(-180));

        ImageAsserts.Similar(transformed2, transformed, 0);
        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    // TODO: Add tests for rotating black and white images

    [Fact]
    public void ScaleNull()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog);

        var transformed = original.PerformTransform(new ScaleTransform());

        ImageAsserts.Similar(expected, transformed, ImageAsserts.NULL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void Scale50Percent()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_sc_50pct);

        var transformed = original.PerformTransform(new ScaleTransform(0.5));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.XPLAT_RMSE_THRESHOLD, ignoreResolution: true);
        AssertOwnership(original, transformed);
    }

    [Theory]
    [MemberData(nameof(CommutativeGrayTransforms))]
    public void GrayTransformsAreCommutative(Transform transform)
    {
        var original = LoadImage(ImageResources.dog);

        var transformed = original.CopyWithPixelFormat(ImagePixelFormat.Gray8);
        transformed = transformed.PerformTransform(transform);

        var expected = original.Clone();
        expected = expected.PerformTransform(transform);
        expected = expected.CopyWithPixelFormat(ImagePixelFormat.Gray8);

        ImageAsserts.Similar(expected, transformed);
    }

    [Fact]
    public void Scale1000Percent()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_huge);

        var transformed = original.PerformTransform(new ScaleTransform(10));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.XPLAT_RMSE_THRESHOLD, ignoreResolution: true);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void Scale50PercentWithAlpha()
    {
        var original = LoadImage(ImageResources.dog_alpha);
        var expected = LoadImage(ImageResources.dog_alpha_sc_50pct);

        var transformed = original.PerformTransform(new ScaleTransform(0.5));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.XPLAT_RMSE_THRESHOLD, ignoreResolution: true);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void CropNull()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog);

        var transformed = original.PerformTransform(new CropTransform());

        ImageAsserts.Similar(expected, transformed, ImageAsserts.NULL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void Crop()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_c_5_10_15_20);

        var transformed = original.PerformTransform(new CropTransform(10, 20, 15, 5));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void CropBlackWhiteBeforeAfter()
    {
        var first = LoadImage(ImageResources.dog);
        var second = LoadImage(ImageResources.dog);

        first = first.PerformTransform(new BlackWhiteTransform());
        first = first.PerformTransform(new CropTransform(10, 20, 15, 5));
        second = second.PerformTransform(new CropTransform(10, 20, 15, 5));
        second = second.PerformTransform(new BlackWhiteTransform());

        ImageAsserts.Similar(first, second, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void CropWithOriginal()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_c_5_10_15_20);

        var transformed = original.PerformTransform(new CropTransform(10, 20, 15, 5, original.Width, original.Height));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void CropWithDifferentOriginal()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_c_5_10_15_20);

        var transformed = ImageContext.PerformTransform(original,
            new CropTransform(20, 40, 30, 10, original.Width * 2, original.Height * 2));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void CropOutOfBounds()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog);

        var transformed = original.PerformTransform(new CropTransform(-1, -1, -1, -1));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void BlackWhite()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_bw);

        var transformed = original.PerformTransform(new BlackWhiteTransform());
        Assert.Equal(ImagePixelFormat.BW1, transformed.UpdateLogicalPixelFormat());

        // TODO: There's no inherent reason this shouldn't be an exact match, unless I guess if
        // there's a slight pixel difference between png loading on mac/gdi
        ImageAsserts.Similar(expected, transformed, ImageAsserts.XPLAT_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void BlackWhiteP300()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_bw_p300);

        var transformed = original.PerformTransform(new BlackWhiteTransform(300));
        Assert.Equal(ImagePixelFormat.BW1, transformed.UpdateLogicalPixelFormat());

        ImageAsserts.Similar(expected, transformed, ImageAsserts.XPLAT_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void Grayscale()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_gray);

        var transformed = original.PerformTransform(new GrayscaleTransform());
        Assert.Equal(ImagePixelFormat.Gray8, transformed.UpdateLogicalPixelFormat());

        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void ColorBitDepth()
    {
        var original = LoadImage(ImageResources.dog_bw);
        var expected = LoadImage(ImageResources.dog_bw_24bit);

        var transformed = original.PerformTransform(new BlackWhiteTransform());
        transformed = transformed.PerformTransform(new ColorBitDepthTransform());
        Assert.Equal(ImagePixelFormat.RGB24, transformed.PixelFormat);

        ImageAsserts.Similar(expected, transformed, ImageAsserts.NULL_RMSE_THRESHOLD);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void Thumbnail()
    {
        var original = LoadImage(ImageResources.dog);
        var expected = LoadImage(ImageResources.dog_thumb_256);

        var transformed = original.PerformTransform(new ThumbnailTransform(256));

        ImageAsserts.Similar(expected, transformed, ImageAsserts.XPLAT_RMSE_THRESHOLD, ignoreResolution: true);
        AssertOwnership(original, transformed);
    }

    [Fact]
    public void Combine()
    {
        var first = LoadImage(ImageResources.dog);
        var second = LoadImage(ImageResources.cat);
        var expected = LoadImage(ImageResources.dog_cat_combined);

        var transformed = MoreImageTransforms.Combine(first, second, CombineOrientation.Vertical);
        Assert.Equal(ImagePixelFormat.RGB24, transformed.UpdateLogicalPixelFormat());

        ImageAsserts.Similar(expected, transformed, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void CombineBlackAndWhite()
    {
        var first = LoadImage(ImageResources.dog).PerformTransform(new BlackWhiteTransform());
        var second = LoadImage(ImageResources.cat).PerformTransform(new BlackWhiteTransform());
        var expected = LoadImage(ImageResources.dog_cat_combined_bw).PerformTransform(new BlackWhiteTransform());

        var transformed = MoreImageTransforms.Combine(first, second, CombineOrientation.Vertical);
        Assert.Equal(ImagePixelFormat.BW1, transformed.UpdateLogicalPixelFormat());

        ImageAsserts.Similar(expected, transformed, ImageAsserts.XPLAT_RMSE_THRESHOLD);
    }

    private void AssertOwnership(IMemoryImage original, IMemoryImage transformed)
    {
        // The contract for a transform is that either it returns the original image or it disposes the original and
        // returns a copy. This check works in both cases, and tests what we really care about (that disposing the
        // result cleans up everything).
        Assert.False(IsDisposed(transformed));
        transformed.Dispose();
        Assert.True(IsDisposed(original));
    }

    public static IEnumerable<object[]> CommutativeGrayTransforms =
    [
        // Note that hue and saturation aren't commutative with grayscale as the the grayscale transform weighs each
        // color channel differently
        new object[] { new BrightnessTransform(300) },
        new object[] { new TrueContrastTransform(300) },
        new object[] { new ScaleTransform(0.5) },
        new object[] { new RotationTransform(46) },
        new object[] { new ThumbnailTransform() },
        new object[] { new CropTransform(10, 10, 10, 10) }
    ];
}