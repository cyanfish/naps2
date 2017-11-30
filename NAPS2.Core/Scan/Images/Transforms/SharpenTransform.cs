using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAPS2.Scan.Images.Transforms
{
    [Serializable]
    public class SharpenTransform : Transform
    {
        public int Sharpness { get; set; }

        public override Bitmap Perform(Bitmap bitmap)
        {
            double sharpnessAdjusted = Sharpness / 1000.0;

            EnsurePixelFormat(ref bitmap);
            int bytesPerPixel;
            if (bitmap.PixelFormat == PixelFormat.Format24bppRgb)
            {
                bytesPerPixel = 3;
            } else if (bitmap.PixelFormat == PixelFormat.Format32bppArgb)
            {
                bytesPerPixel = 4;
            }
            else
            {
                return bitmap;
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

            return bitmap;
        }

        public override bool IsNull => Sharpness == 0;
    }
}
