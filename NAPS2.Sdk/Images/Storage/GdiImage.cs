using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using NAPS2.Images.Transforms;

namespace NAPS2.Images.Storage
{
    public class GdiImage : IImage
    {
        static GdiImage()
        {
            StorageManager.RegisterConverters(new GdiConverters());
            StorageManager.RegisterImageFactory<GdiImage>(new GdiImageFactory());
            Transform.RegisterTransformers<GdiImage>(new GdiTransformers());
        }

        public GdiImage(Bitmap bitmap)
        {
            Bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
        }

        public Bitmap Bitmap { get; }

        public int Width => Bitmap.Width;

        public int Height => Bitmap.Height;

        public float HorizontalResolution => Bitmap.HorizontalResolution;

        public float VerticalResolution => Bitmap.VerticalResolution;

        public void SetResolution(float xDpi, float yDpi)
        {
            if (xDpi > 0 && yDpi > 0)
            {
                Bitmap.SetResolution(xDpi, yDpi);
            }
        }

        public StoragePixelFormat PixelFormat
        {
            get
            {
                switch (Bitmap.PixelFormat)
                {
                    case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                        return StoragePixelFormat.RGB24;
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                        return StoragePixelFormat.ARGB32;
                    case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
                        return StoragePixelFormat.BW1;
                    default:
                        return StoragePixelFormat.Unsupported;
                }
            }
        }

        public bool IsOriginalLossless => Equals(Bitmap.RawFormat, ImageFormat.Bmp) || Equals(Bitmap.RawFormat, ImageFormat.Png);

        public object Lock(out IntPtr scan0, out int stride)
        {
            var bitmapData = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), ImageLockMode.ReadWrite, Bitmap.PixelFormat);
            scan0 = bitmapData.Scan0;
            stride = Math.Abs(bitmapData.Stride);
            return bitmapData;
        }

        public void Unlock(object state)
        {
            var bitmapData = (BitmapData)state;
            Bitmap.UnlockBits(bitmapData);
        }

        public void Dispose()
        {
            Bitmap.Dispose();
        }

        public IImage Clone()
        {
            return new GdiImage((Bitmap)Bitmap.Clone());
        }
    }
}
