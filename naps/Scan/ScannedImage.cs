/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009        Pavel Sorejs
    Copyright (C) 2012, 2013  Ben Olden-Cooligan

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
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace NAPS2.Scan
{
    public class ScannedImage : IScannedImage
    {
        private ScanBitDepth bitDepth;
        private ImageFormat imageFormat;

        private Bitmap baseImage;
        private MemoryStream baseImageEncoded;
        private Bitmap thumbnail;

        private const int thumbnailWidth = 128;
        private const int thumbnailHeight = 128;

        private static Bitmap resizeBitmap(Bitmap b, int nWidth, int nHeight)
        {
            Bitmap result = new Bitmap(nWidth, nHeight);
            Graphics g = Graphics.FromImage((Image)result);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            if (b.Width > b.Height)
            {
                double nheight = (double)b.Height * ((double)nWidth / (double)b.Width);
                double ntop = ((double)nHeight - nheight) / 2;
                g.DrawImage(b, 0, (int)ntop, nWidth, (int)nheight);
            }
            else
            {
                double nwidth = (double)b.Width * ((double)nHeight / (double)b.Height);
                double nleft = ((double)nWidth - nwidth) / 2;
                g.DrawImage(b, (int)nleft, 0, (int)nwidth, nHeight);
            }

            g.Dispose();

            return result;
        }

        public ScannedImage(Bitmap img, ScanBitDepth bitDepth, ImageFormat imageFormat)
        {
            this.bitDepth = bitDepth;
            this.imageFormat = imageFormat;
            thumbnail = resizeBitmap(img, thumbnailWidth, thumbnailHeight);

            if (bitDepth == ScanBitDepth.BLACKWHITE)
            {
                SetBaseImage(CImageHelper.CopyToBpp((Bitmap)img, 1));
            }
            else
            {
                SetBaseImage(img);
            }
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
            if (bitDepth == ScanBitDepth.BLACKWHITE)
            {
                return baseImage;
            }
            else
            {
                return new Bitmap(baseImageEncoded);
            }
        }

        private void SetBaseImage(Bitmap bitmap)
        {
            if (bitDepth == ScanBitDepth.BLACKWHITE)
            {
                baseImage = bitmap;
            }
            else
            {
                if (baseImageEncoded != null)
                {
                    baseImageEncoded.Dispose();
                }
                baseImageEncoded = new MemoryStream();
                baseImage.Save(baseImageEncoded, imageFormat);
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
            using (var img = GetImage())
            {
                img.RotateFlip(rotateFlipType);
                thumbnail = resizeBitmap(img, thumbnailWidth, thumbnailHeight);
                SetBaseImage(img);
            }
        }
    }
}
