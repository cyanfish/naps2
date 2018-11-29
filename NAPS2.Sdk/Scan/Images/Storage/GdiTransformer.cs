using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.Scan.Images.Storage
{
    public class GdiTransformer :
        ITransformer<GdiStorage, BrightnessTransform>,
        ITransformer<GdiStorage, ContrastTransform>,
        ITransformer<GdiStorage, TrueContrastTransform>,
        ITransformer<GdiStorage, HueTransform>,
        ITransformer<GdiStorage, SaturationTransform>,
        ITransformer<GdiStorage, SharpenTransform>,
        ITransformer<GdiStorage, RotationTransform>,
        ITransformer<GdiStorage, CropTransform>,
        ITransformer<GdiStorage, BlackWhiteTransform>,
        ITransformer<GdiStorage, ThumbnailTransform>,
        ITransformer<GdiStorage, ScaleTransform>
    {
        public GdiStorage PerformTransform(GdiStorage storage, BrightnessTransform transform)
        {
            var bitmap = storage.Bitmap;
            float brightnessAdjusted = transform.Brightness / 1000f;
            EnsurePixelFormat(ref bitmap);
            UnsafeImageOps.ChangeBrightness(bitmap, brightnessAdjusted);
            return new GdiStorage(bitmap);
        }

        public GdiStorage PerformTransform(GdiStorage storage, ContrastTransform transform)
        {
            float contrastAdjusted = transform.Contrast / 1000f + 1.0f;

            var bitmap = storage.Bitmap;
            EnsurePixelFormat(ref bitmap);
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
            return storage;
        }

        public GdiStorage PerformTransform(GdiStorage storage, TrueContrastTransform transform)
        {
            // convert +/-1000 input range to a logarithmic scaled multiplier
            float contrastAdjusted = (float)Math.Pow(2.718281f, transform.Contrast / 500.0f);
            // see http://docs.rainmeter.net/tips/colormatrix-guide/ for offset & matrix calculation
            float offset = (1.0f - contrastAdjusted) / 2.0f;

            var bitmap = storage.Bitmap;
            EnsurePixelFormat(ref bitmap);
            UnsafeImageOps.ChangeContrast(bitmap, contrastAdjusted, offset);
            // TODO: Actually need to create a new storage. Change signature of EnsurePixelFormat.
            return storage;
        }

        public GdiStorage PerformTransform(GdiStorage storage, HueTransform transform)
        {
            if (storage.PixelFormat != StoragePixelFormat.RGB24 && storage.PixelFormat != StoragePixelFormat.ARGB32)
            {
                // No need to handle 1bpp since hue shifts are null transforms
                return storage;
            }

            float hueShiftAdjusted = transform.HueShift / 2000f * 360;
            if (hueShiftAdjusted < 0)
            {
                hueShiftAdjusted += 360;
            }

            UnsafeImageOps.HueShift(storage.Bitmap, hueShiftAdjusted);

            return storage;
        }

        public GdiStorage PerformTransform(GdiStorage storage, SaturationTransform transform)
        {
            double saturationAdjusted = transform.Saturation / 1000.0 + 1;

            var bitmap = storage.Bitmap;
            EnsurePixelFormat(ref bitmap);
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
                return storage;
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

            return storage;
        }

        public GdiStorage PerformTransform(GdiStorage storage, SharpenTransform transform)
        {
            double sharpnessAdjusted = transform.Sharpness / 1000.0;

            var bitmap = storage.Bitmap;
            EnsurePixelFormat(ref bitmap);
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
                return storage;
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

            return storage;
        }

        public GdiStorage PerformTransform(GdiStorage storage, RotationTransform transform)
        {

            if (Math.Abs(transform.Angle - 0.0) < RotationTransform.TOLERANCE)
            {
                return storage;
            }
            if (Math.Abs(transform.Angle - 90.0) < RotationTransform.TOLERANCE)
            {
                storage.Bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                return storage;
            }
            if (Math.Abs(transform.Angle - 180.0) < RotationTransform.TOLERANCE)
            {
                storage.Bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                return storage;
            }
            if (Math.Abs(transform.Angle - 270.0) < RotationTransform.TOLERANCE)
            {
                storage.Bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                return storage;
            }
            Bitmap result;
            if (transform.Angle > 45.0 && transform.Angle < 135.0 || transform.Angle > 225.0 && transform.Angle < 315.0)
            {
                result = new Bitmap(storage.Height, storage.Width);
                result.SafeSetResolution(storage.VerticalResolution, storage.HorizontalResolution);
            }
            else
            {
                result = new Bitmap(storage.Width, storage.Height);
                result.SafeSetResolution(storage.HorizontalResolution, storage.VerticalResolution);
            }
            using (var g = Graphics.FromImage(result))
            {
                g.Clear(Color.White);
                g.TranslateTransform(result.Width / 2.0f, result.Height / 2.0f);
                g.RotateTransform((float)transform.Angle);
                g.TranslateTransform(-storage.Width / 2.0f, -storage.Height / 2.0f);
                g.DrawImage(storage.Bitmap, new Rectangle(0, 0, storage.Width, storage.Height));
            }
            OptimizePixelFormat(storage.Bitmap, ref result);
            storage.Dispose();
            return new GdiStorage(result);
        }

        public GdiStorage PerformTransform(GdiStorage storage, CropTransform transform)
        {
            double xScale = storage.Width / (double)(transform.OriginalWidth ?? storage.Width),
                yScale = storage.Height / (double)(transform.OriginalHeight ?? storage.Height);

            int width = Math.Max(storage.Width - (int)Math.Round((transform.Left + transform.Right) * xScale), 1);
            int height = Math.Max(storage.Height - (int)Math.Round((transform.Top + transform.Bottom) * yScale), 1);
            var result = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            result.SafeSetResolution(storage.HorizontalResolution, storage.VerticalResolution);
            using (var g = Graphics.FromImage(result))
            {
                g.Clear(Color.White);
                int x = (int) Math.Round(-transform.Left * xScale);
                int y = (int) Math.Round(-transform.Top * yScale);
                g.DrawImage(storage.Bitmap, new Rectangle(x, y, storage.Width, storage.Height));
            }
            OptimizePixelFormat(storage.Bitmap, ref result);
            storage.Dispose();
            return new GdiStorage(result);
        }

        public GdiStorage PerformTransform(GdiStorage storage, ScaleTransform transform)
        {
            double realWidth = storage.Width / transform.ScaleFactor;
            double realHeight = storage.Height / transform.ScaleFactor;

            double horizontalRes = storage.HorizontalResolution / transform.ScaleFactor;
            double verticalRes = storage.VerticalResolution / transform.ScaleFactor;

            var result = new Bitmap((int)realWidth, (int)realHeight, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(storage.Bitmap, 0, 0, (int)realWidth, (int)realHeight);
                result.SafeSetResolution((float)horizontalRes, (float)verticalRes);
                return new GdiStorage(result);
            }
        }

        public GdiStorage PerformTransform(GdiStorage storage, BlackWhiteTransform transform)
        {
            if (storage.PixelFormat != StoragePixelFormat.RGB24 && storage.PixelFormat != StoragePixelFormat.ARGB32)
            {
                return storage;
            }

            var monoBitmap = UnsafeImageOps.ConvertTo1Bpp(storage.Bitmap, transform.Threshold);
            storage.Dispose();

            return new GdiStorage(monoBitmap);
        }

        /// <summary>
        /// Gets a bitmap resized to fit within a thumbnail rectangle, including a border around the picture.
        /// </summary>
        /// <param name="storage">The bitmap to resize.</param>
        /// <param name="transform">The maximum width and height of the thumbnail.</param>
        /// <returns>The thumbnail bitmap.</returns>
        public GdiStorage PerformTransform(GdiStorage storage, ThumbnailTransform transform)
        {
            var result = new Bitmap(transform.Size, transform.Size);
            using (Graphics g = Graphics.FromImage(result))
            {
                // The location and dimensions of the old bitmap, scaled and positioned within the thumbnail bitmap
                int left, top, width, height;

                // We want a nice thumbnail, so use the maximum quality interpolation
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                if (storage.Width > storage.Height)
                {
                    // Fill the new bitmap's width
                    width = transform.Size;
                    left = 0;
                    // Scale the drawing height to match the original bitmap's aspect ratio
                    height = (int)(storage.Height * (transform.Size / (double)storage.Width));
                    // Center the drawing vertically
                    top = (transform.Size - height) / 2;
                }
                else
                {
                    // Fill the new bitmap's height
                    height = transform.Size;
                    top = 0;
                    // Scale the drawing width to match the original bitmap's aspect ratio
                    width = (int)(storage.Width * (transform.Size / (double)storage.Height));
                    // Center the drawing horizontally
                    left = (transform.Size - width) / 2;
                }

                // Draw the original bitmap onto the new bitmap, using the calculated location and dimensions
                // Note that there may be some padding if the aspect ratios don't match
                var destRect = new RectangleF(left, top, width, height);
                var srcRect = new RectangleF(0, 0, storage.Width, storage.Height);
                g.DrawImage(storage.Bitmap, destRect, srcRect, GraphicsUnit.Pixel);
                // Draw a border around the orignal bitmap's content, inside the padding
                g.DrawRectangle(Pens.Black, left, top, width - 1, height - 1);
            }

            return new GdiStorage(result);
        }

        /// <summary>
        /// If the provided bitmap is 1-bit (black and white), replace it with a 24-bit bitmap so that image transforms will work. If the bitmap is replaced, the original is disposed.
        /// </summary>
        /// <param name="bitmap">The bitmap that may be replaced.</param>
        protected static void EnsurePixelFormat(ref Bitmap bitmap)
        {
            if (bitmap.PixelFormat == PixelFormat.Format1bppIndexed)
            {
                // Copy B&W over to grayscale
                var bitmap2 = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
                bitmap2.SafeSetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
                using (var g = Graphics.FromImage(bitmap2))
                {
                    g.DrawImage(bitmap, 0, 0);
                }
                bitmap.Dispose();
                bitmap = bitmap2;
            }
        }

        /// <summary>
        /// If the original bitmap is 1-bit (black and white), optimize the result by making it 1-bit too.
        /// </summary>
        /// <param name="original">The original bitmap that is used to determine whether the result should be black and white.</param>
        /// <param name="result">The result that may be replaced.</param>
        protected static void OptimizePixelFormat(Bitmap original, ref Bitmap result)
        {
            if (original.PixelFormat == PixelFormat.Format1bppIndexed)
            {
                var bitmap2 = (Bitmap)BitmapHelper.CopyToBpp(result, 1).Clone();
                result.Dispose();
                result = bitmap2;
            }
        }
    }
}
