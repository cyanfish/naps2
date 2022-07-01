using System.Drawing;
using Google.Protobuf;
using NAPS2.Images.Gdi;
using NAPS2.Remoting.Worker;
using NAPS2.Scan.Internal.Twain;
using NAPS2.Sdk.Tests.Asserts;
using NTwain.Data;
using Xunit;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace NAPS2.Sdk.Tests.Scan;

public class TwainMemoryBufferReaderTests
{
    [Fact]
    public void ColorImage()
    {
        var buffer = new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(
                0xFF, 0x00, 0x00,
                0x00, 0xFF, 0x00,
                0x00, 0x00,
                0x00, 0x00, 0xFF,
                0xFF, 0xFF, 0xFF,
                0x00, 0x00),
            Columns = 2,
            Rows = 2,
            BytesPerRow = 8,
            XOffset = 0,
            YOffset = 0
        };
        var imageData = CreateColorImageData(2, 2);
        var image = Create24BitImage(2, 2);

        TwainMemoryBufferReader.CopyBufferToImage(buffer, imageData, image);

        ImageAsserts.PixelColors(image, new()
        {
            {(0, 0), Color.Red},
            {(1, 0), Color.Lime},
            {(0, 1), Color.Blue},
            {(1, 1), Color.White},
        });
    }

    [Fact]
    public void GrayscaleImage()
    {
        var buffer = new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(
                0xFF, 0x00, 0x00, 0x00,
                0x80, 0xD3, 0x00, 0x00),
            Columns = 2,
            Rows = 2,
            BytesPerRow = 4,
            XOffset = 0,
            YOffset = 0
        };
        var imageData = CreateGrayscaleImageData(2, 2);
        var image = Create24BitImage(2, 2);

        TwainMemoryBufferReader.CopyBufferToImage(buffer, imageData, image);

        ImageAsserts.PixelColors(image, new()
        {
            {(0, 0), Color.White},
            {(1, 0), Color.Black},
            {(0, 1), Color.Gray},
            {(1, 1), Color.LightGray},
        });
    }

    [Fact]
    public void BlackWhiteImage()
    {
        var buffer = new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(
                0x80, 0x00, 0x00, 0x00,
                0x40, 0x00, 0x00, 0x00),
            Columns = 2,
            Rows = 2,
            BytesPerRow = 4,
            XOffset = 0,
            YOffset = 0
        };
        var imageData = CreateBlackWhiteImageData(2, 2);
        var image = Create1BitImage(2, 2);

        TwainMemoryBufferReader.CopyBufferToImage(buffer, imageData, image);

        ImageAsserts.PixelColors(image, new()
        {
            {(0, 0), Color.White},
            {(1, 0), Color.Black},
            {(0, 1), Color.Black},
            {(1, 1), Color.White},
        });
    }
    
    // TODO: Add tests for strips/tiles, error cases

    private static TwainImageData CreateColorImageData(int width, int height)
    {
        return new TwainImageData
        {
            Height = height,
            Width = width,
            PixelType = (int) PixelType.RGB,
            BitsPerPixel = 24,
            BitsPerSample = { 8, 8, 8 },
            SamplesPerPixel = 3
        };
    }

    private static TwainImageData CreateGrayscaleImageData(int width, int height)
    {
        return new TwainImageData
        {
            Height = height,
            Width = width,
            PixelType = (int) PixelType.Gray,
            BitsPerPixel = 8,
            BitsPerSample = { 8, 0, 0 },
            SamplesPerPixel = 1
        };
    }

    private static TwainImageData CreateBlackWhiteImageData(int width, int height)
    {
        return new TwainImageData
        {
            Height = height,
            Width = width,
            PixelType = (int) PixelType.BlackWhite,
            BitsPerPixel = 1,
            BitsPerSample = { 1, 0, 0 },
            SamplesPerPixel = 1
        };
    }

    private static GdiImage Create24BitImage(int width, int height)
    {
        return new GdiImage(new Bitmap(width, height, PixelFormat.Format24bppRgb));
    }

    private static GdiImage Create1BitImage(int width, int height)
    {
        return new GdiImage(new Bitmap(width, height, PixelFormat.Format1bppIndexed));
    }
}