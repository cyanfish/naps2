using NAPS2.Images.Bitwise;

namespace NAPS2.Images.Transforms;

public static class MoreImageTransforms
{
    public static IMemoryImage Combine(IMemoryImage first, IMemoryImage second, CombineOrientation orientation,
        double offset = 0.5)
    {
        var imageContext = first.ImageContext;
        var pixelFormat = first.PixelFormat > second.PixelFormat
            ? first.PixelFormat
            : second.PixelFormat;
        int width = orientation == CombineOrientation.Horizontal
            ? first.Width + second.Width
            : Math.Max(first.Width, second.Width);
        int height = orientation == CombineOrientation.Vertical
            ? first.Height + second.Height
            : Math.Max(first.Height, second.Height);

        var combinedImage = imageContext.Create(width, height, pixelFormat);
        combinedImage.SetResolution(
            Math.Max(first.HorizontalResolution, second.HorizontalResolution),
            Math.Max(first.VerticalResolution, second.VerticalResolution));

        FillColorImageOp.White.Perform(combinedImage);

        new CopyBitwiseImageOp
        {
            DestXOffset = orientation == CombineOrientation.Horizontal ? 0 :
                first.Width > second.Width ? 0 :
                (int) (offset * (second.Width - first.Width)),
            DestYOffset = orientation == CombineOrientation.Vertical ? 0 :
                first.Height > second.Height ? 0 :
                (int) (offset * (second.Height - first.Height))
        }.Perform(first, combinedImage);

        new CopyBitwiseImageOp
        {
            DestXOffset = orientation == CombineOrientation.Horizontal ? first.Width :
                second.Width > first.Width ? 0 :
                (int) (offset * (first.Width - second.Width)),
            DestYOffset = orientation == CombineOrientation.Vertical ? first.Height :
                second.Height > first.Height ? 0 :
                (int) (offset * (first.Height - second.Height))
        }.Perform(second, combinedImage);

        return combinedImage;
    }
}