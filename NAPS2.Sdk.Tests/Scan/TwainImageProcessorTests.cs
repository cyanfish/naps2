using Google.Protobuf;
using NAPS2.Remoting.Worker;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Scan.Internal.Twain;
using NAPS2.Sdk.Tests.Asserts;
using NSubstitute;
using NTwain.Data;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class TwainImageProcessorTests : ContextualTests
{
    private static readonly (int, int, int) RED = (0xFF, 0, 0);
    private static readonly (int, int, int) GREEN = (0, 0xFF, 0);
    private static readonly (int, int, int) BLUE = (0, 0, 0xFF);
    private static readonly (int, int, int) WHITE = (0xFF, 0xFF, 0xFF);
    private static readonly (int, int, int) BLACK = (0, 0, 0);
    private static readonly (int, int, int) GRAY = (0x80, 0x80, 0x80);
    private static readonly (int, int, int) LIGHT_GRAY = (0xD3, 0xD3, 0xD3);

    private readonly IScanEvents _scanEvents;
    private readonly Action<IMemoryImage> _callback;
    private readonly TwainImageProcessor _processor;
    private readonly List<IMemoryImage> _images;

    public TwainImageProcessorTests()
    {
        _scanEvents = Substitute.For<IScanEvents>();
        _callback = Substitute.For<Action<IMemoryImage>>();

        _images = new List<IMemoryImage>();
        _callback.When(x => x(Arg.Any<IMemoryImage>()))
            .Do(x => _images.Add((IMemoryImage) x[0]));

        _processor = new TwainImageProcessor(ScanningContext, new ScanOptions(), _scanEvents, _callback);
    }

    [Fact]
    public void SingleImageSingleBuffer()
    {
        _processor.PageStart(new TwainPageStart
        {
            ImageData = CreateColorImageData(2, 2)
        });
        _processor.MemoryBufferTransferred(new TwainMemoryBuffer
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
        });
        _processor.Flush();

        Assert.Single(_images);
        ImageAsserts.PixelColors(_images[0], new()
        {
            { (0, 0), RED },
            { (1, 0), GREEN },
            { (0, 1), BLUE },
            { (1, 1), WHITE },
        });
    }

    [Fact]
    public void SingleImageTwoBuffers()
    {
        _processor.PageStart(new TwainPageStart
        {
            ImageData = CreateColorImageData(2, 2)
        });
        _processor.MemoryBufferTransferred(new TwainMemoryBuffer
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
        });
        _processor.MemoryBufferTransferred(new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(
                0x00, 0x00, 0xFF,
                0xFF, 0xFF, 0xFF,
                0x00, 0x00),
            Columns = 2,
            Rows = 1,
            BytesPerRow = 8,
            XOffset = 0,
            YOffset = 1
        });
        _processor.Flush();

        Assert.Single(_images);
        ImageAsserts.PixelColors(_images[0], new()
        {
            { (0, 0), RED },
            { (1, 0), GREEN },
            { (0, 1), BLUE },
            { (1, 1), WHITE },
        });
    }

    [Fact]
    public void MultipleImages()
    {
        _processor.PageStart(new TwainPageStart
        {
            ImageData = CreateColorImageData(2, 2)
        });
        _processor.MemoryBufferTransferred(new TwainMemoryBuffer
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
        });
        _processor.PageStart(new TwainPageStart
        {
            ImageData = CreateColorImageData(2, 2)
        });
        _processor.MemoryBufferTransferred(new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(
                0x00, 0x00, 0x00,
                0x80, 0x80, 0x80,
                0x00, 0x00,
                0xD3, 0xD3, 0xD3,
                0xFF, 0xFF, 0xFF,
                0x00, 0x00),
            Columns = 2,
            Rows = 2,
            BytesPerRow = 8,
            XOffset = 0,
            YOffset = 0
        });
        _processor.Flush();

        Assert.Equal(2, _images.Count);
        ImageAsserts.PixelColors(_images[0], new()
        {
            { (0, 0), RED },
            { (1, 0), GREEN },
            { (0, 1), BLUE },
            { (1, 1), WHITE },
        });
        ImageAsserts.PixelColors(_images[1], new()
        {
            { (0, 0), BLACK },
            { (1, 0), GRAY },
            { (0, 1), LIGHT_GRAY },
            { (1, 1), WHITE },
        });
    }

    [Fact]
    public void SingleImageTooSmall()
    {
        _processor.PageStart(new TwainPageStart
        {
            ImageData = CreateColorImageData(3, 3)
        });
        _processor.MemoryBufferTransferred(new TwainMemoryBuffer
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
        });
        _processor.Flush();

        Assert.Single(_images);
        Assert.Equal(2, _images[0].Width);
        Assert.Equal(2, _images[0].Height);
        ImageAsserts.PixelColors(_images[0], new()
        {
            { (0, 0), RED },
            { (1, 0), GREEN },
            { (0, 1), BLUE },
            { (1, 1), WHITE },
        });
    }

    [Fact]
    public void SingleImageTooBig()
    {
        _processor.PageStart(new TwainPageStart
        {
            ImageData = CreateColorImageData(1, 1)
        });
        _processor.MemoryBufferTransferred(new TwainMemoryBuffer
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
        });
        _processor.Flush();

        Assert.Single(_images);
        Assert.Equal(2, _images[0].Width);
        Assert.Equal(2, _images[0].Height);
        ImageAsserts.PixelColors(_images[0], new()
        {
            { (0, 0), RED },
            { (1, 0), GREEN },
            { (0, 1), BLUE },
            { (1, 1), WHITE },
        });
    }

    [Fact]
    public void MultipleImagesWrongSize()
    {
        _processor.PageStart(new TwainPageStart
        {
            ImageData = CreateColorImageData(3, 3)
        });
        _processor.MemoryBufferTransferred(new TwainMemoryBuffer
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
        });
        _processor.PageStart(new TwainPageStart
        {
            ImageData = CreateColorImageData(1, 1)
        });
        _processor.MemoryBufferTransferred(new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(
                0x00, 0x00, 0x00,
                0x80, 0x80, 0x80,
                0x00, 0x00,
                0xD3, 0xD3, 0xD3,
                0xFF, 0xFF, 0xFF,
                0x00, 0x00),
            Columns = 2,
            Rows = 2,
            BytesPerRow = 8,
            XOffset = 0,
            YOffset = 0
        });
        _processor.Flush();

        Assert.Equal(2, _images.Count);
        Assert.Equal(2, _images[0].Width);
        Assert.Equal(2, _images[0].Height);
        ImageAsserts.PixelColors(_images[0], new()
        {
            { (0, 0), RED },
            { (1, 0), GREEN },
            { (0, 1), BLUE },
            { (1, 1), WHITE },
        });
        Assert.Equal(2, _images[1].Width);
        Assert.Equal(2, _images[1].Height);
        ImageAsserts.PixelColors(_images[1], new()
        {
            { (0, 0), BLACK },
            { (1, 0), GRAY },
            { (0, 1), LIGHT_GRAY },
            { (1, 1), WHITE },
        });
    }

    [Fact]
    public void DisposeFlushesWithFullImage()
    {
        _processor.PageStart(new TwainPageStart
        {
            ImageData = CreateColorImageData(2, 2)
        });
        _processor.MemoryBufferTransferred(new TwainMemoryBuffer
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
        });
        _processor.Dispose();

        Assert.Single(_images);
        ImageAsserts.PixelColors(_images[0], new()
        {
            { (0, 0), RED },
            { (1, 0), GREEN },
            { (0, 1), BLUE },
            { (1, 1), WHITE },
        });
    }

    [Fact]
    public void DisposeDoesntFlushWithPartialImage()
    {
        _processor.PageStart(new TwainPageStart
        {
            ImageData = CreateColorImageData(2, 2)
        });
        _processor.MemoryBufferTransferred(new TwainMemoryBuffer
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
        });
        _processor.Dispose();

        Assert.Empty(_images);
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
}