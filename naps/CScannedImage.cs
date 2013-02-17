using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace NAPS
{
    public class CScannedImage : IDisposable
    {
        private Bitmap baseImage;
        private MemoryStream baseImageEncoded;
        private Bitmap thumbnail;

        private int thumbnailWidth = 128;
        private int thumbnailHeight = 128;
        private CScanSettings.BitDepth bitDepth;

        public CScanSettings.BitDepth BitDepth
        {
            get { return bitDepth; }
            set { bitDepth = value; }
        }

        public Bitmap Thumbnail
        {
            get
            {
                return thumbnail;
            }
        }

        public Bitmap GetBaseImage()
        {
            if (baseImage != null)
            {
                return (Bitmap) baseImage.Clone();
            }
            else
            {
                Bitmap img = new Bitmap(baseImageEncoded);
                return img;
            }
        }

        private Bitmap resizeBitmap(Bitmap b, int nWidth, int nHeight)
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

        public CScannedImage(Bitmap img, CScanSettings.BitDepth BitDepth, bool highQuality)
        {
            thumbnail = resizeBitmap(img, thumbnailWidth, thumbnailHeight);

            if (bitDepth == CScanSettings.BitDepth.BLACKWHITE)
            {
                baseImage = CImageHelper.CopyToBpp((Bitmap)img, 1);
                img.Dispose();
            }
            else
            {
                baseImageEncoded = new MemoryStream();
                if (highQuality)
                {
                    img.Save(baseImageEncoded, ImageFormat.Png);
                }
                else
                {
                    img.Save(baseImageEncoded, ImageFormat.Jpeg);
                }
            }
            this.BitDepth = BitDepth;
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

        internal void RotateFlip(RotateFlipType rotateFlipType)
        {
            baseImage.RotateFlip(rotateFlipType);
            thumbnail = resizeBitmap(baseImage, thumbnailWidth, thumbnailHeight);
        }
    }
}
