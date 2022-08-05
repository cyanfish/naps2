using MonoMac.CoreGraphics;

namespace NAPS2.Images.Mac;

public class MacImageTransformer : AbstractImageTransformer<MacImage>
{
    public MacImageTransformer(ImageContext imageContext) : base(imageContext)
    {
    }

    protected override MacImage PerformTransform(MacImage image, ContrastTransform transform)
    {
        throw new NotImplementedException();
    }

    protected override MacImage PerformTransform(MacImage image, SaturationTransform transform)
    {
        throw new NotImplementedException();
    }

    protected override MacImage PerformTransform(MacImage image, SharpenTransform transform)
    {
        throw new NotImplementedException();
    }

    protected override MacImage PerformTransform(MacImage image, RotationTransform transform)
    {
        MacImage newImage;
        if (transform.Angle > 45.0 && transform.Angle < 135.0 || transform.Angle > 225.0 && transform.Angle < 315.0)
        {
            newImage = (MacImage)ImageContext.Create(image.Height, image.Width, ImagePixelFormat.ARGB32);
            newImage.SetResolution(image.VerticalResolution, image.HorizontalResolution);
        }
        else
        {
            newImage = (MacImage)ImageContext.Create(image.Width, image.Height, ImagePixelFormat.ARGB32);
            newImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
        }
        using CGBitmapContext c = GetCgBitmapContext(newImage);
        c.TranslateCTM(-image.Width / 2.0, -image.Height / 2.0);
        c.RotateCTM(-transform.Angle * Math.PI / 180);
        c.TranslateCTM(newImage.Width / 2.0, newImage.Height / 2.0);
        CGRect rect = new CGRect(0, 0, image.Width, image.Height);
        c.DrawImage(new CGRect(0, 0, image.Width, image.Height), image._imageRep.AsCGImage(ref rect, null, null));
        return newImage;
    }

    private static CGBitmapContext GetCgBitmapContext(MacImage image)
    {
        lock (MacImageContext.ConstructorLock)
        {
            return new CGBitmapContext(
                image._imageRep.BitmapData,
                image.Width,
                image.Height,
                (int)image._imageRep.BitsPerSample,
                (int)image._imageRep.BytesPerRow,
                CGColorSpace.CreateDeviceRGB(),
                CGImageAlphaInfo.PremultipliedLast);
        }
    }

    protected override MacImage PerformTransform(MacImage image, CropTransform transform)
    {
        throw new NotImplementedException();
    }

    protected override MacImage PerformTransform(MacImage image, ScaleTransform transform)
    {
        throw new NotImplementedException();
    }

    protected override MacImage PerformTransform(MacImage image, ThumbnailTransform transform)
    {
        throw new NotImplementedException();
    }
}