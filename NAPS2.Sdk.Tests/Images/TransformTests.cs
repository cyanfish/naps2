using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class TransformTests : ContextualTexts
{
    // JPEG artifacts seem to consistently create a RMSE of about 2.5.
    // TODO: Use PNG or some other way to do a precise comparison.
    private const double GENERAL_RMSE_THRESHOLD = 3.5;

    private const double NULL_RMSE_THRESHOLD = 0.5;

    // TODO: Test handling of other pixel formats
    // ARGB32 -> should work (ignoring alpha channel)
    // BW1 -> should work where applicable
    // Unsupported -> should throw an exception
    // This might require some actual changes to the transforms.

    [Fact]
    public void BrightnessNull()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image);

        actual = ImageContext.PerformTransform(actual, new BrightnessTransform());

        ImageAsserts.Similar(expected, actual, NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void BrightnessP300()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_b_p300);

        actual = ImageContext.PerformTransform(actual, new BrightnessTransform(300));

        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void BrightnessN300()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_b_n300);

        actual = ImageContext.PerformTransform(actual, new BrightnessTransform(-300));

        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void ContrastNull()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image);

        actual = ImageContext.PerformTransform(actual, new TrueContrastTransform());

        ImageAsserts.Similar(expected, actual, NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void ContrastP300()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_c_p300);

        actual = ImageContext.PerformTransform(actual, new TrueContrastTransform(300));

        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void ContrastN300()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_c_n300);

        actual = ImageContext.PerformTransform(actual, new TrueContrastTransform(-300));

        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void HueNull()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image);

        actual = ImageContext.PerformTransform(actual, new HueTransform());

        ImageAsserts.Similar(expected, actual, NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void HueP300()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_h_p300);

        actual = ImageContext.PerformTransform(actual, new HueTransform(300));

        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void HueN300()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_h_n300);

        actual = ImageContext.PerformTransform(actual, new HueTransform(-300));

        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SaturationNull()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image);

        actual = ImageContext.PerformTransform(actual, new SaturationTransform());

        ImageAsserts.Similar(expected, actual, NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SaturationP300()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_s_p300);

        actual = ImageContext.PerformTransform(actual, new SaturationTransform(300));

        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SaturationN300()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_s_n300);

        actual = ImageContext.PerformTransform(actual, new SaturationTransform(-300));

        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SharpenNull()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image);

        actual = ImageContext.PerformTransform(actual, new SharpenTransform());

        ImageAsserts.Similar(expected, actual, NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SharpenP300()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_sh_p1000);

        actual = ImageContext.PerformTransform(actual, new SharpenTransform(1000));

        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void SharpenN300()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_sh_n1000);

        actual = ImageContext.PerformTransform(actual, new SharpenTransform(-1000));

        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void RotationNull()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image);

        actual = ImageContext.PerformTransform(actual, new RotationTransform());

        ImageAsserts.Similar(expected, actual, NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void RotationP90()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_r_p90);

        actual = ImageContext.PerformTransform(actual, new RotationTransform(90));

        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void RotationP46()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_r_p46);

        actual = ImageContext.PerformTransform(actual, new RotationTransform(46));

        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void RotationN45()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_r_n45);

        actual = ImageContext.PerformTransform(actual, new RotationTransform(-45));

        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void Rotation180()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage actual2 = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_r_180);

        actual = ImageContext.PerformTransform(actual, new RotationTransform(180));
        actual2 = ImageContext.PerformTransform(actual2, new RotationTransform(-180));

        ImageAsserts.Similar(actual2, actual, 0);
        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void CropNull()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image);

        actual = ImageContext.PerformTransform(actual, new CropTransform());

        ImageAsserts.Similar(expected, actual, NULL_RMSE_THRESHOLD);
    }

    [Fact]
    public void Crop()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_c_5_10_15_20);

        actual = ImageContext.PerformTransform(actual, new CropTransform(10, 20, 15, 5));

        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void CropWithOriginal()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_c_5_10_15_20);

        actual = ImageContext.PerformTransform(actual, new CropTransform(10, 20, 15, 5, actual.Width, actual.Height));

        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void CropWithDifferentOriginal()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_c_5_10_15_20);

        actual = ImageContext.PerformTransform(actual, new CropTransform(20, 40, 30, 10, actual.Width * 2, actual.Height * 2));

        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }
        
    [Fact]
    public void BlackWhite()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_bw);

        actual = ImageContext.PerformTransform(actual, new BlackWhiteTransform());
        Assert.Equal(ImagePixelFormat.BW1, actual.PixelFormat);

        actual = To24Bit(actual);
        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    [Fact]
    public void BlackWhiteP300()
    {
        IImage actual = new GdiImage(TransformTestsData.color_image);
        IImage expected = new GdiImage(TransformTestsData.color_image_bw_p300);

        actual = ImageContext.PerformTransform(actual, new BlackWhiteTransform(300));
        Assert.Equal(ImagePixelFormat.BW1, actual.PixelFormat);

        actual = To24Bit(actual);
        ImageAsserts.Similar(expected, actual, GENERAL_RMSE_THRESHOLD);
    }

    private static IImage To24Bit(IImage actual)
    {
        // Convert to 24-bit for comparison
        // TODO: Maybe have a Color24BitTransform or something to be more reusable
        var bitmap = ((GdiImage) actual).Bitmap;
        GdiTransformers.EnsurePixelFormat(ref bitmap);
        actual = new GdiImage(bitmap);
        return actual;
    }
}