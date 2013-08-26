/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace NAPS2.Scan
{
    public class ScannedImage : IScannedImage
    {
        private const int THUMBNAIL_WIDTH = 128;
        private const int THUMBNAIL_HEIGHT = 128;

        // The image's bit depth (or C24Bit if unknown)
        private readonly ScanBitDepth bitDepth;
        // Only one of the following (baseImage/baseImageEncoded) should have a value for any particular ScannedImage
        private readonly Bitmap baseImage;
        private readonly MemoryStream baseImageEncoded;
        // Store a base image and transform pair (rather than doing the actual transform on the base image)
        // so that JPEG degradation is minimized when multiple rotations/flips are performed
        private RotateFlipType transform = RotateFlipType.RotateNoneFlipNone;

        public ScannedImage(Bitmap img, ScanBitDepth bitDepth, bool highQuality)
        {
            this.bitDepth = bitDepth;
            Thumbnail = GetThumbnail(img);

            // Store the image in as little space as possible
            if (bitDepth == ScanBitDepth.BlackWhite)
            {
                // Store as a 1-bit bitmap
                // This is lossless and takes up minimal storage (best of both worlds), so highQuality is irrelevant
                baseImage = (Bitmap)ImageHelper.CopyToBpp(img, 1).Clone();
                // Note that if a black and white image comes from native WIA, bitDepth is unknown,
                // so the image will be png-encoded below instead of using a 1-bit bitmap
            }
            else if (highQuality)
            {
                // Store as PNG
                // Lossless, but some images (color/grayscale) take up lots of storage
                baseImageEncoded = EncodeBitmap(img, ImageFormat.Png);
            }
            else
            {
                // Store as PNG/JPEG depending on which is smaller
                var pngEncoded = EncodeBitmap(img, ImageFormat.Png);
                var jpegEncoded = EncodeBitmap(img, ImageFormat.Jpeg);
                if (pngEncoded.Length <= jpegEncoded.Length)
                {
                    // Probably a black and white image (from native WIA, so bitDepth is unknown), which PNG compresses well vs. JPEG
                    baseImageEncoded = pngEncoded;
                    jpegEncoded.Dispose();
                }
                else
                {
                    // Probably a color or grayscale image, which JPEG compresses well vs. PNG
                    baseImageEncoded = jpegEncoded;
                    pngEncoded.Dispose();
                }
            }
        }

        private static MemoryStream EncodeBitmap(Bitmap bitmap, ImageFormat imageFormat)
        {
            var encoded = new MemoryStream();
            bitmap.Save(encoded, imageFormat);
            return encoded;
        }

        public Bitmap Thumbnail { get; private set; }

        public Bitmap GetImage()
        {
            var bitmap = bitDepth == ScanBitDepth.BlackWhite ? (Bitmap)baseImage.Clone() : new Bitmap(baseImageEncoded);
            bitmap.RotateFlip(transform);
            return bitmap;
        }

        public void Dispose()
        {
            if (baseImage != null)
            {
                baseImage.Dispose();
            }
            if (baseImageEncoded != null)
            {
                baseImageEncoded.Dispose();
            }
            Thumbnail.Dispose();
        }

        public void RotateFlip(RotateFlipType rotateFlipType)
        {
            transform = CombineTransform(transform, rotateFlipType);
            Thumbnail.RotateFlip(rotateFlipType);
        }

        private static RotateFlipType CombineTransform(RotateFlipType currentTransform, RotateFlipType nextTransform)
        {
            // There should be no actual flips (just rotations of varying degrees), so this code is simplified
            Debug.Assert((int)currentTransform < 4); // 0-3 are rotations of varying degrees (0, 90, 180, 270)
            Debug.Assert((int)nextTransform < 4);
            return FromRotation(GetRotation(currentTransform) + GetRotation(nextTransform));
        }

        private static int GetRotation(RotateFlipType rotateFlipType)
        {
            switch (rotateFlipType)
            {
                case RotateFlipType.RotateNoneFlipNone:
                    return 0;
                case RotateFlipType.Rotate90FlipNone:
                    return 1;
                case RotateFlipType.Rotate180FlipNone:
                    return 2;
                case RotateFlipType.Rotate270FlipNone:
                    return 3;
            }
            throw new ArgumentException();
        }

        private static RotateFlipType FromRotation(int rotation)
        {
            switch (rotation % 4)
            {
                case 0:
                    return RotateFlipType.RotateNoneFlipNone;
                case 1:
                    return RotateFlipType.Rotate90FlipNone;
                case 2:
                    return RotateFlipType.Rotate180FlipNone;
                case 3:
                    return RotateFlipType.Rotate270FlipNone;
            }
            throw new ArgumentException();
        }

        /// <summary>
        /// Gets a bitmap resized to fit within a thumbnail rectangle, including a border around the picture.
        /// </summary>
        /// <param name="b">The bitmap to resize.</param>
        /// <returns>The thumbnail bitmap.</returns>
        private static Bitmap GetThumbnail(Bitmap b)
        {
            var result = new Bitmap(THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT);
            using (Graphics g = Graphics.FromImage(result))
            {
                // The location and dimensions of the old bitmap, scaled and positioned within the thumbnail bitmap
                int left, top, width, height;

                // We want a nice thumbnail, so use the maximum quality interpolation
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                if (b.Width > b.Height)
                {
                    // Fill the new bitmap's width
                    width = THUMBNAIL_WIDTH;
                    left = 0;
                    // Scale the drawing height to match the original bitmap's aspect ratio
                    height = (int)(b.Height * (THUMBNAIL_WIDTH / (double)b.Width));
                    // Center the drawing vertically
                    top = (THUMBNAIL_HEIGHT - height) / 2;
                }
                else
                {
                    // Fill the new bitmap's height
                    height = THUMBNAIL_HEIGHT;
                    top = 0;
                    // Scale the drawing width to match the original bitmap's aspect ratio
                    width = (int)(b.Width * (THUMBNAIL_HEIGHT / (double)b.Height));
                    // Center the drawing horizontally
                    left = (THUMBNAIL_WIDTH - width) / 2;
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
