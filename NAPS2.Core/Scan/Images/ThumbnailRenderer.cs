using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.Scan.Images
{
    public class ThumbnailRenderer
    {
        public const int MIN_SIZE = 64;
        public const int DEFAULT_SIZE = 128;
        public static int MAX_SIZE = 1024;

        public static double StepNumberToSize(double stepNumber)
        {
            // 64-256:32:6 256-448:48:4 448-832:64:6 832-1024:96:2
            if (stepNumber < 6)
            {
                return 64 + stepNumber * 32;
            }
            if (stepNumber < 10)
            {
                return 256 + (stepNumber - 6) * 48;
            }
            if (stepNumber < 16)
            {
                return 448 + (stepNumber - 10) * 64;
            }
            return 832 + (stepNumber - 16) * 96;
        }

        public static double SizeToStepNumber(double size)
        {
            if (size < 256)
            {
                return (size - 64) / 32;
            }
            if (size < 448)
            {
                return (size - 256) / 48 + 6;
            }
            if (size < 832)
            {
                return (size - 448) / 64 + 10;
            }
            return (size - 832) / 96 + 16;
        }

        private readonly IUserConfigManager userConfigManager;
        private readonly ScannedImageRenderer scannedImageRenderer;

        public ThumbnailRenderer(IUserConfigManager userConfigManager, ScannedImageRenderer scannedImageRenderer)
        {
            this.userConfigManager = userConfigManager;
            this.scannedImageRenderer = scannedImageRenderer;
        }

        public Bitmap RenderThumbnail(ScannedImage scannedImage)
        {
            return RenderThumbnail(scannedImageRenderer.Render(scannedImage), userConfigManager.Config.ThumbnailSize);
        }

        public Bitmap RenderThumbnail(ScannedImage scannedImage, int size)
        {
            return RenderThumbnail(scannedImageRenderer.Render(scannedImage), size);
        }

        public Bitmap RenderThumbnail(Bitmap b)
        {
            return RenderThumbnail(b, userConfigManager.Config.ThumbnailSize);
        }

        /// <summary>
        /// Gets a bitmap resized to fit within a thumbnail rectangle, including a border around the picture.
        /// </summary>
        /// <param name="b">The bitmap to resize.</param>
        /// <param name="size">The maximum width and height of the thumbnail.</param>
        /// <returns>The thumbnail bitmap.</returns>
        public virtual Bitmap RenderThumbnail(Bitmap b, int size)
        {
            var result = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(result))
            {
                // The location and dimensions of the old bitmap, scaled and positioned within the thumbnail bitmap
                int left, top, width, height;

                // We want a nice thumbnail, so use the maximum quality interpolation
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                if (b.Width > b.Height)
                {
                    // Fill the new bitmap's width
                    width = size;
                    left = 0;
                    // Scale the drawing height to match the original bitmap's aspect ratio
                    height = (int)(b.Height * (size / (double)b.Width));
                    // Center the drawing vertically
                    top = (size - height) / 2;
                }
                else
                {
                    // Fill the new bitmap's height
                    height = size;
                    top = 0;
                    // Scale the drawing width to match the original bitmap's aspect ratio
                    width = (int)(b.Width * (size / (double)b.Height));
                    // Center the drawing horizontally
                    left = (size - width) / 2;
                }

                // Draw the original bitmap onto the new bitmap, using the calculated location and dimensions
                // Note that there may be some padding if the aspect ratios don't match
                g.DrawImage(b, left, top, width, height);
                // Draw a border around the orignal bitmap's content, inside the padding
                g.DrawRectangle(Pens.Black, left, top, width - 1, height - 1);
            }
            return result;
        }
    }
}