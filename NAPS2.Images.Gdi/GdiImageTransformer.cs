using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace NAPS2.Images.Gdi;

public class GdiImageTransformer : AbstractImageTransformer<GdiImage>
{
    public GdiImageTransformer(ImageContext imageContext) : base(imageContext)
    {
    }

    protected override GdiImage PerformTransform(GdiImage image, ContrastTransform transform)
    {
        float contrastAdjusted = transform.Contrast / 1000f + 1.0f;

        EnsurePixelFormat(ref image);
        var bitmap = image.Bitmap;
        using (var g = Graphics.FromImage(bitmap))
        {
            var attrs = new ImageAttributes();
            attrs.SetColorMatrix(new ColorMatrix
            {
                Matrix00 = contrastAdjusted,
                Matrix11 = contrastAdjusted,
                Matrix22 = contrastAdjusted
            });
            g.DrawImage(bitmap,
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                0,
                0,
                bitmap.Width,
                bitmap.Height,
                GraphicsUnit.Pixel,
                attrs);
        }
        return image;
    }


    protected override GdiImage PerformTransform(GdiImage image, SaturationTransform transform)
    {
        double saturationAdjusted = transform.Saturation / 1000.0 + 1;

        EnsurePixelFormat(ref image);
        var bitmap = image.Bitmap;
        int bytesPerPixel;
        if (bitmap.PixelFormat == PixelFormat.Format24bppRgb)
        {
            bytesPerPixel = 3;
        }
        else if (bitmap.PixelFormat == PixelFormat.Format32bppArgb)
        {
            bytesPerPixel = 4;
        }
        else
        {
            return image;
        }

        var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
        var stride = Math.Abs(data.Stride);
        for (int y = 0; y < data.Height; y++)
        {
            for (int x = 0; x < data.Width; x++)
            {
                int r = Marshal.ReadByte(data.Scan0 + stride * y + x * bytesPerPixel);
                int g = Marshal.ReadByte(data.Scan0 + stride * y + x * bytesPerPixel + 1);
                int b = Marshal.ReadByte(data.Scan0 + stride * y + x * bytesPerPixel + 2);

                Color c = Color.FromArgb(255, r, g, b);
                ColorHelper.ColorToHSL(c, out double h, out double s, out double v);

                s = Math.Min(s * saturationAdjusted, 1);

                c = ColorHelper.ColorFromHSL(h, s, v);

                Marshal.WriteByte(data.Scan0 + stride * y + x * bytesPerPixel, c.R);
                Marshal.WriteByte(data.Scan0 + stride * y + x * bytesPerPixel + 1, c.G);
                Marshal.WriteByte(data.Scan0 + stride * y + x * bytesPerPixel + 2, c.B);
            }
        }
        bitmap.UnlockBits(data);

        return image;
    }


    protected override GdiImage PerformTransform(GdiImage image, SharpenTransform transform)
    {
        double sharpnessAdjusted = transform.Sharpness / 1000.0;

        EnsurePixelFormat(ref image);
        var bitmap = image.Bitmap;
        int bytesPerPixel;
        if (bitmap.PixelFormat == PixelFormat.Format24bppRgb)
        {
            bytesPerPixel = 3;
        }
        else if (bitmap.PixelFormat == PixelFormat.Format32bppArgb)
        {
            bytesPerPixel = 4;
        }
        else
        {
            return image;
        }

        // From https://stackoverflow.com/a/17596299

        int width = bitmap.Width;
        int height = bitmap.Height;

        // Create sharpening filter.
        const int filterSize = 5;

        var filter = new double[,]
        {
            {-1, -1, -1, -1, -1},
            {-1,  2,  2,  2, -1},
            {-1,  2, 16,  2, -1},
            {-1,  2,  2,  2, -1},
            {-1, -1, -1, -1, -1}
        };

        double bias = 1.0 - sharpnessAdjusted;
        double factor = sharpnessAdjusted / 16.0;

        const int s = filterSize / 2;

        var result = new Color[bitmap.Width, bitmap.Height];

        // Lock image bits for read/write.
        BitmapData pbits = bitmap.LockBits(new Rectangle(0, 0, width, height),
            ImageLockMode.ReadWrite,
            bitmap.PixelFormat);

        // Declare an array to hold the bytes of the bitmap.
        int bytes = pbits.Stride * height;
        var rgbValues = new byte[bytes];

        // Copy the RGB values into the array.
        Marshal.Copy(pbits.Scan0, rgbValues, 0, bytes);

        int rgb;
        // Fill the color array with the new sharpened color values.
        for (int x = s; x < width - s; x++)
        {
            for (int y = s; y < height - s; y++)
            {
                double red = 0.0, green = 0.0, blue = 0.0;

                for (int filterX = 0; filterX < filterSize; filterX++)
                {
                    for (int filterY = 0; filterY < filterSize; filterY++)
                    {
                        int imageX = (x - s + filterX + width) % width;
                        int imageY = (y - s + filterY + height) % height;

                        rgb = imageY * pbits.Stride + bytesPerPixel * imageX;

                        red += rgbValues[rgb + 2] * filter[filterX, filterY];
                        green += rgbValues[rgb + 1] * filter[filterX, filterY];
                        blue += rgbValues[rgb + 0] * filter[filterX, filterY];
                    }

                    rgb = y * pbits.Stride + bytesPerPixel * x;

                    int r = Math.Min(Math.Max((int)(factor * red + (bias * rgbValues[rgb + 2])), 0), 255);
                    int g = Math.Min(Math.Max((int)(factor * green + (bias * rgbValues[rgb + 1])), 0), 255);
                    int b = Math.Min(Math.Max((int)(factor * blue + (bias * rgbValues[rgb + 0])), 0), 255);

                    result[x, y] = Color.FromArgb(r, g, b);
                }
            }
        }

        // Update the image with the sharpened pixels.
        for (int x = s; x < width - s; x++)
        {
            for (int y = s; y < height - s; y++)
            {
                rgb = y * pbits.Stride + bytesPerPixel * x;

                rgbValues[rgb + 2] = result[x, y].R;
                rgbValues[rgb + 1] = result[x, y].G;
                rgbValues[rgb + 0] = result[x, y].B;
            }
        }

        // Copy the RGB values back to the bitmap.
        Marshal.Copy(rgbValues, 0, pbits.Scan0, bytes);
        // Release image bits.
        bitmap.UnlockBits(pbits);

        return image;
    }

    protected override GdiImage PerformTransform(GdiImage image, RotationTransform transform)
    {
        if (Math.Abs(transform.Angle - 0.0) < RotationTransform.TOLERANCE)
        {
            return image;
        }
        if (Math.Abs(transform.Angle - 90.0) < RotationTransform.TOLERANCE)
        {
            image.Bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            return image;
        }
        if (Math.Abs(transform.Angle - 180.0) < RotationTransform.TOLERANCE)
        {
            image.Bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
            return image;
        }
        if (Math.Abs(transform.Angle - 270.0) < RotationTransform.TOLERANCE)
        {
            image.Bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
            return image;
        }
        Bitmap result;
        if (transform.Angle > 45.0 && transform.Angle < 135.0 || transform.Angle > 225.0 && transform.Angle < 315.0)
        {
            result = new Bitmap(image.Height, image.Width, PixelFormat.Format24bppRgb);
            result.SafeSetResolution(image.VerticalResolution, image.HorizontalResolution);
        }
        else
        {
            result = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
            result.SafeSetResolution(image.HorizontalResolution, image.VerticalResolution);
        }
        using (var g = Graphics.FromImage(result))
        {
            g.Clear(Color.White);
            g.TranslateTransform(result.Width / 2.0f, result.Height / 2.0f);
            g.RotateTransform((float)transform.Angle);
            g.TranslateTransform(-image.Width / 2.0f, -image.Height / 2.0f);
            g.DrawImage(image.Bitmap, new Rectangle(0, 0, image.Width, image.Height));
        }
        var resultImage = new GdiImage(result);
        OptimizePixelFormat(image, ref resultImage);
        image.Dispose();
        return resultImage;
    }

    protected override GdiImage PerformTransform(GdiImage image, ScaleTransform transform)
    {
        int realWidth = (int)Math.Round(image.Width * transform.ScaleFactor);
        int realHeight = (int)Math.Round(image.Height * transform.ScaleFactor);

        double horizontalRes = image.HorizontalResolution * transform.ScaleFactor;
        double verticalRes = image.VerticalResolution * transform.ScaleFactor;

        var result = new Bitmap(realWidth, realHeight, PixelFormat.Format24bppRgb);
        using Graphics g = Graphics.FromImage(result);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawImage(image.Bitmap, 0, 0, realWidth, realHeight);
        result.SafeSetResolution((float)horizontalRes, (float)verticalRes);
        return new GdiImage(result);
    }

    protected override GdiImage PerformTransform(GdiImage image, ThumbnailTransform transform)
    {
        var result = new Bitmap(transform.Size, transform.Size);
        using (Graphics g = Graphics.FromImage(result))
        {
            // The location and dimensions of the old bitmap, scaled and positioned within the thumbnail bitmap
            int left, top, width, height;

            // We want a nice thumbnail, so use the maximum quality interpolation
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            if (image.Width > image.Height)
            {
                // Fill the new bitmap's width
                width = transform.Size;
                left = 0;
                // Scale the drawing height to match the original bitmap's aspect ratio
                height = (int)(image.Height * (transform.Size / (double)image.Width));
                // Center the drawing vertically
                top = (transform.Size - height) / 2;
            }
            else
            {
                // Fill the new bitmap's height
                height = transform.Size;
                top = 0;
                // Scale the drawing width to match the original bitmap's aspect ratio
                width = (int)(image.Width * (transform.Size / (double)image.Height));
                // Center the drawing horizontally
                left = (transform.Size - width) / 2;
            }

            // Draw the original bitmap onto the new bitmap, using the calculated location and dimensions
            // Note that there may be some padding if the aspect ratios don't match
            var destRect = new RectangleF(left, top, width, height);
            var srcRect = new RectangleF(0, 0, image.Width, image.Height);
            g.DrawImage(image.Bitmap, destRect, srcRect, GraphicsUnit.Pixel);
            // Draw a border around the original bitmap's content, inside the padding
            g.DrawRectangle(Pens.Black, left, top, width - 1, height - 1);
        }
        return new GdiImage(result);
    }
}