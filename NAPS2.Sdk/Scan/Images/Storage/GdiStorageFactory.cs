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
        public IMemoryStorage Decode(Stream stream, string ext) => new GdiStorage(new Bitmap(stream));

        public IMemoryStorage Decode(string path) => new GdiStorage(new Bitmap(path));

        public IEnumerable<IMemoryStorage> DecodeMultiple(Stream stream, string ext, out int count)
        {
            var bitmap = new Bitmap(stream);
            count = bitmap.GetFrameCount(FrameDimension.Page);
            return EnumerateFrames(bitmap, count);
        }

        public IEnumerable<IMemoryStorage> DecodeMultiple(string path, out int count)
        {
            var bitmap = new Bitmap(path);
            count = bitmap.GetFrameCount(FrameDimension.Page);
            return EnumerateFrames(bitmap, count);
        }

        private IEnumerable<IMemoryStorage> EnumerateFrames(Bitmap bitmap, int count)
        {
            using (bitmap)
            {
                for (int i = 0; i < count; i++)
                {
                    bitmap.SelectActiveFrame(FrameDimension.Page, i);
                    yield return new GdiStorage((Bitmap) bitmap.Clone());
                }
            }
        }

        public IMemoryStorage FromDimensions(int width, int height, StoragePixelFormat pixelFormat) => new GdiStorage(new Bitmap(width, height, GdiPixelFormat(pixelFormat)));

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
