using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace NAPS2.Scan.Images
{
    internal static class ThumbnailHelper
    {
        /// <summary>
        /// Gets a bitmap resized to fit within a thumbnail rectangle, including a border around the picture.
        /// </summary>
        /// <param name="b">The bitmap to resize.</param>
        /// <param name="size">The maximum width and height of the thumbnail.</param>
        /// <returns>The thumbnail bitmap.</returns>
        public static Bitmap GetThumbnail(Bitmap b, int size)
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