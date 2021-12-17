using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace NAPS2.Images.Storage;

public class GdiImageFactory : IImageFactory
{
    public IImage Decode(Stream stream, string ext) => new GdiImage(new Bitmap(stream));

    public IImage Decode(string path) => new GdiImage(new Bitmap(path));

    public IEnumerable<IImage> DecodeMultiple(Stream stream, string ext, out int count)
    {
        var bitmap = new Bitmap(stream);
        count = bitmap.GetFrameCount(FrameDimension.Page);
        return EnumerateFrames(bitmap, count);
    }

    public IEnumerable<IImage> DecodeMultiple(string path, out int count)
    {
        var bitmap = new Bitmap(path);
        count = bitmap.GetFrameCount(FrameDimension.Page);
        return EnumerateFrames(bitmap, count);
    }

    private IEnumerable<IImage> EnumerateFrames(Bitmap bitmap, int count)
    {
        using (bitmap)
        {
            for (int i = 0; i < count; i++)
            {
                bitmap.SelectActiveFrame(FrameDimension.Page, i);
                yield return new GdiImage((Bitmap) bitmap.Clone());
            }
        }
    }

    public IImage FromDimensions(int width, int height, StoragePixelFormat pixelFormat)
    {
        var bitmap = new Bitmap(width, height, GdiPixelFormat(pixelFormat));
        if (pixelFormat == StoragePixelFormat.BW1)
        {
            var p = bitmap.Palette;
            p.Entries[0] = Color.Black;
            p.Entries[1] = Color.White;
            bitmap.Palette = p;
        }
        return new GdiImage(bitmap);
    }

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