/* NAPS2 (Not Another PDF Scanner 2)
 * Copyright (C) 2009  Pavel Sorejs
 * Copyright (C) 2012-2013 Ben Olden-Cooligan
 * Licensed under the GNU General Public License Version 2
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace NAPS
{
    public class CScannedImage : IScannedImage
    {
        private CScanSettings.BitDepth bitDepth;
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

        public CScannedImage(Bitmap img, CScanSettings.BitDepth bitDepth, ImageFormat imageFormat)
        {
            this.bitDepth = bitDepth;
            this.imageFormat = imageFormat;
            thumbnail = resizeBitmap(img, thumbnailWidth, thumbnailHeight);

            if (bitDepth == CScanSettings.BitDepth.BLACKWHITE)
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
            if (bitDepth == CScanSettings.BitDepth.BLACKWHITE)
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
            if (bitDepth == CScanSettings.BitDepth.BLACKWHITE)
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
