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
        private readonly ScanBitDepth bitDepth;

        private readonly Bitmap baseImage;
        private readonly MemoryStream baseImageEncoded;
        private RotateFlipType transform = RotateFlipType.RotateNoneFlipNone;

        public ScannedImage(Bitmap img, ScanBitDepth bitDepth, bool highQuality)
        {
            this.bitDepth = bitDepth;
            Thumbnail = ResizeBitmap(img, THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT);

            // Store the image in as little space as possible
            if (bitDepth == ScanBitDepth.BlackWhite)
            {
                // Store as a 1-bit bitmap
                // This is lossless and takes up minimal storage (best of both worlds), so highQuality is irrelevant
                baseImage = (Bitmap)ImageHelper.CopyToBpp(img, 1).Clone();
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
                    // Probably a black and white image (e.g. from TWAIN or native WIA), which PNG compresses well vs. JPEG
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
            Debug.Assert((int)currentTransform < 4);
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

        private static Bitmap ResizeBitmap(Bitmap b, int newWidth, int newHeight)
        {
            var result = new Bitmap(newWidth, newHeight);
            Graphics g = Graphics.FromImage(result);

            int left, top, width, height;

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            if (b.Width > b.Height)
            {
                width = newWidth;
                height = (int)(b.Height * (newWidth / (double)b.Width));
                left = 0;
                top = (newHeight - height) / 2;
            }
            else
            {
                width = (int)(b.Width * (newHeight / (double)b.Height));
                height = newHeight;
                left = (newWidth - width) / 2;
                top = 0;
            }
            g.DrawImage(b, left, top, width, height);
            g.DrawRectangle(Pens.Black, left, top, width - 1, height - 1);

            g.Dispose();

            return result;
        }
    }
}
