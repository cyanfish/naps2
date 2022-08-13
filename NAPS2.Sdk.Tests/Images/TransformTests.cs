using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class TransformTests : ContextualTests
{
    // TODO: Test handling of other pixel formats
    // ARGB32 -> should work (ignoring alpha channel)
    // BW1 -> should work where applicable
    // Unsupported -> should throw an exception
    // This might require some actual changes to the transforms.

    [Fact]
    public void BrightnessNull()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image);

        actual = ImageContext.PerformTransform(actual, new BrightnessTransform());

        ImageAsserts.Similar(expected, actual, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void BrightnessP300()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_b_p300);

        actual = ImageContext.PerformTransform(actual, new BrightnessTransform(300));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void BrightnessN300()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_b_n300);

        actual = ImageContext.PerformTransform(actual, new BrightnessTransform(-300));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void ContrastNull()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image);

        actual = ImageContext.PerformTransform(actual, new TrueContrastTransform());

        ImageAsserts.Similar(expected, actual, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void ContrastP300()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_c_p300);

        actual = ImageContext.PerformTransform(actual, new TrueContrastTransform(300));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void ContrastN300()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_c_n300);

        actual = ImageContext.PerformTransform(actual, new TrueContrastTransform(-300));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void HueNull()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image);

        actual = ImageContext.PerformTransform(actual, new HueTransform());

        ImageAsserts.Similar(expected, actual, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void HueP300()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_h_p300);

        actual = ImageContext.PerformTransform(actual, new HueTransform(300));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void HueN300()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_h_n300);

        actual = ImageContext.PerformTransform(actual, new HueTransform(-300));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SaturationNull()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image);

        actual = ImageContext.PerformTransform(actual, new SaturationTransform());

        ImageAsserts.Similar(expected, actual, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SaturationP300()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_s_p300);

        actual = ImageContext.PerformTransform(actual, new SaturationTransform(300));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SaturationN300()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_s_n300);

        actual = ImageContext.PerformTransform(actual, new SaturationTransform(-300));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SharpenNull()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image);

        actual = ImageContext.PerformTransform(actual, new SharpenTransform());

        ImageAsserts.Similar(expected, actual, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SharpenP300()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_sh_p1000);

        actual = ImageContext.PerformTransform(actual, new SharpenTransform(1000));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SharpenN300()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_sh_n1000);

        actual = ImageContext.PerformTransform(actual, new SharpenTransform(-1000));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void RotationNull()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image);

        actual = ImageContext.PerformTransform(actual, new RotationTransform());

        ImageAsserts.Similar(expected, actual, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void RotationP90()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_r_p90);

        actual = ImageContext.PerformTransform(actual, new RotationTransform(90));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void RotationP46()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_r_p46);

        actual = ImageContext.PerformTransform(actual, new RotationTransform(46));

        ImageAsserts.Similar(expected, actual, ImageAsserts.XPLAT_RMSE_THRESHOLD);
    }

    [Fact]
    public void RotationN45()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_r_n45);

        actual = ImageContext.PerformTransform(actual, new RotationTransform(-45));

        // TODO: The mac rotated image looks way better than gdi, consider if we can improve the gdi end
        ImageAsserts.Similar(expected, actual, ImageAsserts.XPLAT_RMSE_THRESHOLD);
    }

    [Fact]
    public void Rotation180()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage actual2 = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_r_180);

        actual = ImageContext.PerformTransform(actual, new RotationTransform(180));
        actual2 = ImageContext.PerformTransform(actual2, new RotationTransform(-180));

        ImageAsserts.Similar(actual2, actual, 0);
        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void CropNull()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image);

        actual = ImageContext.PerformTransform(actual, new CropTransform());

        ImageAsserts.Similar(expected, actual, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    // TODO: Add test for crop with black & white image (and possibly white & black)
    [Fact]
    public void Crop()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_c_5_10_15_20);

        actual = ImageContext.PerformTransform(actual, new CropTransform(10, 20, 15, 5));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void CropWithOriginal()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_c_5_10_15_20);

        actual = ImageContext.PerformTransform(actual, new CropTransform(10, 20, 15, 5, actual.Width, actual.Height));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void CropWithDifferentOriginal()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_c_5_10_15_20);

        actual = ImageContext.PerformTransform(actual,
            new CropTransform(20, 40, 30, 10, actual.Width * 2, actual.Height * 2));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void CropOutOfBounds()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image);

        actual = ImageContext.PerformTransform(actual, new CropTransform(-1, -1, -1, -1));

        ImageAsserts.Similar(expected, actual, ImageAsserts.GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void BlackWhite()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_bw);

        actual = ImageContext.PerformTransform(actual, new BlackWhiteTransform());
        Assert.Equal(ImagePixelFormat.BW1, actual.PixelFormat);

        // TODO: There's no inherent reason this shouldn't be an exact match, unless I guess if
        // there's a slight pixel difference between png loading on mac/gdi
        ImageAsserts.Similar(expected, actual, ImageAsserts.XPLAT_RMSE_THRESHOLD);
    }

    [Fact]
    public void BlackWhiteP300()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_bw_p300);

        actual = ImageContext.PerformTransform(actual, new BlackWhiteTransform(300));
        Assert.Equal(ImagePixelFormat.BW1, actual.PixelFormat);

        ImageAsserts.Similar(expected, actual, ImageAsserts.XPLAT_RMSE_THRESHOLD);
    }

    [Fact]
    public void ColorBitDepth()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image_bw);
        IMemoryImage expected = LoadImage(ImageResources.color_image_bw_24bit);

        actual = ImageContext.PerformTransform(actual, new ColorBitDepthTransform());
        Assert.Equal(ImagePixelFormat.RGB24, actual.PixelFormat);

        ImageAsserts.Similar(expected, actual, ImageAsserts.NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void Thumbnail()
    {
        IMemoryImage actual = LoadImage(ImageResources.color_image);
        IMemoryImage expected = LoadImage(ImageResources.color_image_thumb_256);

        actual = ImageContext.PerformTransform(actual, new ThumbnailTransform(256));

        // TODO: The image is off by 1 pixel, it might have something to do with how the cggraphics coordinate
        // system is a double and not pixel-aligned
        // TODO: Fix expected resolution
        // TODO: Also fix ThumbnailRendering tests
        ImageAsserts.Similar(expected, actual, ImageAsserts.XPLAT_RMSE_THRESHOLD, ignoreResolution: true);
    }
}