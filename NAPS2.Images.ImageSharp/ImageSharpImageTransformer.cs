using NAPS2.Images.Bitwise;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NAPS2.Images.ImageSharp;

public class ImageSharpImageTransformer : AbstractImageTransformer<ImageSharpImage>
{
    public ImageSharpImageTransformer(ImageContext imageContext) : base(imageContext)
    {
    }

    protected override ImageSharpImage PerformTransform(ImageSharpImage image, RotationTransform transform)
    {
        // TODO: We can maybe optimize this in some ways, e.g. skip a clone if we're already Rgba32, or convert the
        // final pixel format back to whatever the original was.

        var width = image.Width;
        var height = image.Height;
        float xres = image.HorizontalResolution, yres = image.VerticalResolution;
        if (transform.Angle is > 45.0 and < 135.0 or > 225.0 and < 315.0)
        {
            (width, height) = (height, width);
            (xres, yres) = (yres, xres);
        }

        // Get an image with an alpha channel so we can set the background color to white
        var copy = image.Image.CloneAs<Rgba32>();

        copy.Mutate(x => x.Rotate((float) transform.Angle).BackgroundColor(Color.White));

        var cropRect = new Rectangle((copy.Width - width) / 2, (copy.Height - height) / 2, width, height);
        copy.Mutate(x => x.Crop(cropRect));

        var newImage = new ImageSharpImage(ImageContext, copy);
        // TODO: In Gdi, we convert this back to BW1. Should we do the same?
        newImage.LogicalPixelFormat = image.LogicalPixelFormat == ImagePixelFormat.BW1
            ? ImagePixelFormat.Gray8
            : image.LogicalPixelFormat;
        newImage.SetResolution(xres, yres);
        image.Dispose();
        return newImage;
    }

    protected override ImageSharpImage PerformTransform(ImageSharpImage image, ResizeTransform transform)
    {
        image.Image.Mutate(x => x.Resize(transform.Width, transform.Height));
        image.LogicalPixelFormat = image.LogicalPixelFormat == ImagePixelFormat.BW1
            ? ImagePixelFormat.Gray8
            : image.LogicalPixelFormat;
        image.SetResolution(
            image.HorizontalResolution * image.Width / transform.Width,
            image.VerticalResolution * image.Height / transform.Height);
        return image;
    }

    protected override ImageSharpImage PerformTransform(ImageSharpImage image, BlackWhiteTransform transform)
    {
        new DecolorBitwiseImageOp(true)
        {
            BlackWhiteThreshold = (transform.Threshold + 1000) / 2000f
        }.Perform(image);
        var newImage = (ImageSharpImage) image.CopyWithPixelFormat(ImagePixelFormat.Gray8);
        newImage.LogicalPixelFormat = ImagePixelFormat.BW1;
        image.Dispose();
        return newImage;
    }
}