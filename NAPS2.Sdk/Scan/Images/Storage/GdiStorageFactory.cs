using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace NAPS2.Scan.Images.Storage
{
    public class GdiStorageFactory : IMemoryStorageFactory
    {
        public IStorage FromBmpStream(Stream stream) => new GdiStorage(new Bitmap(stream));

        public IStorage FromDimensions(int width, int height, StoragePixelFormat pixelFormat) => new GdiStorage(new Bitmap(width, height, GdiPixelFormat(pixelFormat)));

        private PixelFormat GdiPixelFormat(StoragePixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case StoragePixelFormat.BW1:
                    // TODO: Do we need to set the palette?
                    // TODO: Also maybe it makes sense to have WB1 format too
                    return PixelFormat.Format1bppIndexed;
                case StoragePixelFormat.RGB24:
                    return PixelFormat.Format24bppRgb;
                case StoragePixelFormat.ARGB32:
                    return PixelFormat.Format32bppArgb;
                default:
                    throw new ArgumentException("Pixel format must be specified", nameof(pixelFormat));
            }
        }
    }
}
