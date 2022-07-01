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
    // As TwainMemoryBufferReader is unsafe, it's extra important we have a lot of tests for edge cases as we don't want
    // to crash the whole process if something goes wrong.

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
            { (0, 0), Color.Red },
            { (1, 0), Color.Lime },
            { (0, 1), Color.Blue },
            { (1, 1), Color.White },
        });
    }

    [Fact]
    public void ColorImageStripsAndTiles()
    {
        var buffer1 = new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(
                0xFF, 0x00, 0x00,
                0x00, 0xFF, 0x00,
                0x00, 0x00),
            Columns = 2,
            Rows = 1,
            BytesPerRow = 8,
            XOffset = 0,
            YOffset = 0
        };
        var buffer2 = new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(
                0x00, 0x00, 0xFF, 0x00),
            Columns = 1,
            Rows = 1,
            BytesPerRow = 4,
            XOffset = 0,
            YOffset = 1
        };
        var buffer3 = new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(
                0xFF, 0xFF, 0xFF, 0x00),
            Columns = 1,
            Rows = 1,
            BytesPerRow = 4,
            XOffset = 1,
            YOffset = 1
        };
        var imageData = CreateColorImageData(2, 2);
        var image = Create24BitImage(2, 2);

        TwainMemoryBufferReader.CopyBufferToImage(buffer1, imageData, image);
        TwainMemoryBufferReader.CopyBufferToImage(buffer2, imageData, image);
        TwainMemoryBufferReader.CopyBufferToImage(buffer3, imageData, image);

        ImageAsserts.PixelColors(image, new()
        {
            { (0, 0), Color.Red },
            { (1, 0), Color.Lime },
            { (0, 1), Color.Blue },
            { (1, 1), Color.White },
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
            { (0, 0), Color.White },
            { (1, 0), Color.Black },
            { (0, 1), Color.Gray },
            { (1, 1), Color.LightGray },
        });
    }

    [Fact]
    public void GrayscaleImageStripsAndTiles()
    {
        var buffer1 = new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(
                0xFF, 0x00, 0x00, 0x00),
            Columns = 2,
            Rows = 1,
            BytesPerRow = 4,
            XOffset = 0,
            YOffset = 0
        };
        var buffer2 = new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(
                0x80, 0x00),
            Columns = 1,
            Rows = 1,
            BytesPerRow = 2,
            XOffset = 0,
            YOffset = 1
        };
        var buffer3 = new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(
                0xD3, 0x00),
            Columns = 1,
            Rows = 1,
            BytesPerRow = 2,
            XOffset = 1,
            YOffset = 1
        };
        var imageData = CreateGrayscaleImageData(2, 2);
        var image = Create24BitImage(2, 2);

        TwainMemoryBufferReader.CopyBufferToImage(buffer1, imageData, image);
        TwainMemoryBufferReader.CopyBufferToImage(buffer2, imageData, image);
        TwainMemoryBufferReader.CopyBufferToImage(buffer3, imageData, image);

        ImageAsserts.PixelColors(image, new()
        {
            { (0, 0), Color.White },
            { (1, 0), Color.Black },
            { (0, 1), Color.Gray },
            { (1, 1), Color.LightGray },
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
            { (0, 0), Color.White },
            { (1, 0), Color.Black },
            { (0, 1), Color.Black },
            { (1, 1), Color.White },
        });
    }

    [Fact]
    public void BlackWhiteImageStripsAndTiles()
    {
        var buffer1 = new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(
                0xFF, 0xFF, 0x00, 0x00),
            Columns = 16,
            Rows = 1,
            BytesPerRow = 4,
            XOffset = 0,
            YOffset = 0
        };
        var buffer2 = new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(
                0x00, 0x00, 0x00, 0x00),
            Columns = 8,
            Rows = 1,
            BytesPerRow = 4,
            XOffset = 0,
            YOffset = 1
        };
        var buffer3 = new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(
                0xFF, 0x00, 0x00, 0x00),
            Columns = 8,
            Rows = 1,
            BytesPerRow = 4,
            XOffset = 8,
            YOffset = 1
        };
        var imageData = CreateBlackWhiteImageData(16, 2);
        var image = Create1BitImage(16, 2);

        TwainMemoryBufferReader.CopyBufferToImage(buffer1, imageData, image);
        TwainMemoryBufferReader.CopyBufferToImage(buffer2, imageData, image);
        TwainMemoryBufferReader.CopyBufferToImage(buffer3, imageData, image);

        ImageAsserts.PixelColors(image, new()
        {
            { (0, 0), Color.White },
            { (15, 0), Color.White },
            { (0, 1), Color.Black },
            { (15, 1), Color.White }
        });
    }

    [Fact]
    public void Unaligned1BitOffset()
    {
        var buffer = new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(new byte[8]),
            Columns = 2,
            Rows = 2,
            BytesPerRow = 4,
            XOffset = 1,
            YOffset = 0
        };
        var imageData = CreateBlackWhiteImageData(2, 2);
        var image = Create1BitImage(2, 2);

        Assert.Throws<ArgumentException>(() => TwainMemoryBufferReader.CopyBufferToImage(buffer, imageData, image));
    }

    [Fact]
    public void BufferTooSmall()
    {
        var buffer = new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(new byte[15]),
            Columns = 2,
            Rows = 2,
            BytesPerRow = 8,
            XOffset = 0,
            YOffset = 0
        };
        var imageData = CreateColorImageData(2, 2);
        var image = Create24BitImage(2, 2);

        Assert.Throws<ArgumentException>(() => TwainMemoryBufferReader.CopyBufferToImage(buffer, imageData, image));
    }

    [Fact]
    public void NotEnoughPixelData()
    {
        var buffer = new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(new byte[16]),
            Columns = 2,
            Rows = 2,
            BytesPerRow = 5,
            XOffset = 0,
            YOffset = 0
        };
        var imageData = CreateColorImageData(2, 2);
        var image = Create24BitImage(2, 2);

        Assert.Throws<ArgumentException>(() => TwainMemoryBufferReader.CopyBufferToImage(buffer, imageData, image));
    }

    [Fact]
    public void OffsetTooBig()
    {
        var buffer = new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(new byte[16]),
            Columns = 2,
            Rows = 2,
            BytesPerRow = 8,
            XOffset = 1,
            YOffset = 0
        };
        var imageData = CreateColorImageData(2, 2);
        var image = Create24BitImage(2, 2);

        Assert.Throws<ArgumentException>(() => TwainMemoryBufferReader.CopyBufferToImage(buffer, imageData, image));
    }

    [Fact]
    public void Invalid24BitSamples()
    {
        var buffer = SomeValidBuffer();
        var imageData = new TwainImageData
        {
            Height = 2,
            Width = 2,
            PixelType = (int) PixelType.RGB,
            BitsPerPixel = 24,
            BitsPerSample = { 8, 7, 9 },
            SamplesPerPixel = 3
        };
        var image = Create24BitImage(2, 2);

        Assert.Throws<ArgumentException>(() => TwainMemoryBufferReader.CopyBufferToImage(buffer, imageData, image));
    }

    [Fact]
    public void Invalid8BitPixelType()
    {
        var buffer = SomeValidBuffer();
        var imageData = new TwainImageData
        {
            Height = 2,
            Width = 2,
            PixelType = (int) PixelType.Palette,
            BitsPerPixel = 8,
            BitsPerSample = { 8, 0, 0 },
            SamplesPerPixel = 1
        };
        var image = Create24BitImage(2, 2);

        Assert.Throws<ArgumentException>(() => TwainMemoryBufferReader.CopyBufferToImage(buffer, imageData, image));
    }

    [Fact]
    public void Invalid1BitSamples()
    {
        var buffer = SomeValidBuffer();
        var imageData = new TwainImageData
        {
            Height = 2,
            Width = 2,
            PixelType = (int) PixelType.Palette,
            BitsPerPixel = 1,
            BitsPerSample = { 1, 0, 0 },
            SamplesPerPixel = 2
        };
        var image = Create1BitImage(2, 2);

        Assert.Throws<ArgumentException>(() => TwainMemoryBufferReader.CopyBufferToImage(buffer, imageData, image));
    }

    [Fact]
    public void InvalidBitsPerPixel()
    {
        var buffer = SomeValidBuffer();
        var imageData = new TwainImageData
        {
            Height = 2,
            Width = 2,
            PixelType = (int) PixelType.RGB,
            BitsPerPixel = 32,
            BitsPerSample = { 8, 8, 8, 8 },
            SamplesPerPixel = 4
        };
        var image = Create24BitImage(2, 2);

        Assert.Throws<ArgumentException>(() => TwainMemoryBufferReader.CopyBufferToImage(buffer, imageData, image));
    }

    [Fact]
    public void InvalidColorImagePixelFormat()
    {
        var buffer = SomeValidBuffer();
        var imageData = CreateColorImageData(2, 2);
        var image = Create1BitImage(2, 2);

        Assert.Throws<ArgumentException>(() => TwainMemoryBufferReader.CopyBufferToImage(buffer, imageData, image));
    }

    [Fact]
    public void InvalidGrayscaleImagePixelFormat()
    {
        var buffer = SomeValidBuffer();
        var imageData = CreateGrayscaleImageData(2, 2);
        var image = Create1BitImage(2, 2);

        Assert.Throws<ArgumentException>(() => TwainMemoryBufferReader.CopyBufferToImage(buffer, imageData, image));
    }

    [Fact]
    public void InvalidBlackWhiteImagePixelFormat()
    {
        var buffer = SomeValidBuffer();
        var imageData = CreateBlackWhiteImageData(2, 2);
        var image = Create24BitImage(2, 2);

        Assert.Throws<ArgumentException>(() => TwainMemoryBufferReader.CopyBufferToImage(buffer, imageData, image));
    }

    private static TwainMemoryBuffer SomeValidBuffer()
    {
        return new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(new byte[16]),
            Columns = 2,
            Rows = 2,
            BytesPerRow = 8,
            XOffset = 0,
            YOffset = 0
        };
    }

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