using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace NAPS2.Scan.Images.Storage
{
    public class GdiStorage : IMemoryStorage
    {
        public GdiStorage(Bitmap bitmap)
        {
            Bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
        }

        public Bitmap Bitmap { get; }

        public int Width => Bitmap.Width;

        public int Height => Bitmap.Height;

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

        public object Lock(out IntPtr scan0, out int stride)
        {
            var bitmapData = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), ImageLockMode.ReadWrite, Bitmap.PixelFormat);
            scan0 = bitmapData.Scan0;
            stride = bitmapData.Stride;
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
    }
}
