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
        private readonly ImageFormat imageFormat;

        private Bitmap baseImage;
        private MemoryStream baseImageEncoded;
        private Bitmap thumbnail;

        public ScannedImage(Bitmap img, ScanBitDepth bitDepth, ImageFormat imageFormat)
        {
            this.bitDepth = bitDepth;
            this.imageFormat = imageFormat;
            thumbnail = ResizeBitmap(img, THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT);

            SetBaseImage(bitDepth == ScanBitDepth.BlackWhite ? ImageHelper.CopyToBpp(img, 1) : img);
        }

        public Bitmap Thumbnail
        {
            get
            {
                return thumbnail;
            }
        }

        public Bitmap GetImage()
        {
            if (bitDepth == ScanBitDepth.BlackWhite)
            {
                return (Bitmap)baseImage.Clone();
            }
            else
            {
                return new Bitmap(baseImageEncoded);
            }
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
            thumbnail.Dispose();
        }

        public void RotateFlip(RotateFlipType rotateFlipType)
        {
            using (Bitmap img = GetImage())
            {
                img.RotateFlip(rotateFlipType);
                thumbnail = ResizeBitmap(img, THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT);
                SetBaseImage(img);
            }
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

        private void SetBaseImage(Bitmap bitmap)
        {
            if (bitDepth == ScanBitDepth.BlackWhite)
            {
                baseImage = (Bitmap)bitmap.Clone();
            }
            else
            {
                if (baseImageEncoded != null)
                {
                    baseImageEncoded.Dispose();
                }
                baseImageEncoded = new MemoryStream();
                bitmap.Save(baseImageEncoded, imageFormat);
            }
        }
    }
}
