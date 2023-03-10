#if !MAC
using NAPS2.Images.Bitwise;
using NAPS2.Remoting.Worker;

namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// Run in the main 64-bit process, this class receives the Twain events from the remote Twain session in the worker
/// process and converts them into IMemoryImage objects and page progress events for the IScanDriver interface.
/// </summary>
internal class TwainImageProcessor : ITwainEvents, IDisposable
{
    private readonly ScanningContext _scanningContext;
    private readonly Action<IMemoryImage> _callback;
    private TwainImageData? _currentImageData;
    private IMemoryImage? _currentImage;
    private int _transferredWidth;
    private int _transferredHeight;
    private long _transferredPixels;
    private long _totalPixels;
    private readonly TwainProgressEstimator _progressEstimator;

    public TwainImageProcessor(ScanningContext scanningContext, ScanOptions options, IScanEvents scanEvents,
        Action<IMemoryImage> callback)
    {
        _scanningContext = scanningContext;
        _callback = callback;
        _progressEstimator = new TwainProgressEstimator(options, scanEvents);
    }

    public void PageStart(TwainPageStart pageStart)
    {
        Flush();
        _currentImageData = pageStart.ImageData;
        _currentImage?.Dispose();
        _currentImage = null;
        _transferredWidth = 0;
        _transferredHeight = 0;
        _transferredPixels = 0;
        _totalPixels = _currentImageData == null ? 0 : _currentImageData.Width * (long) _currentImageData.Height;
        _progressEstimator.MarkStart(_totalPixels);
    }

    public void NativeImageTransferred(TwainNativeImage nativeImage)
    {
        using var image = _scanningContext.ImageContext.Load(new MemoryStream(nativeImage.Buffer.ToByteArray()));
        _callback(image);
        _progressEstimator.MarkCompletion();
    }

    public void MemoryBufferTransferred(TwainMemoryBuffer memoryBuffer)
    {
        if (_currentImageData == null)
        {
            throw new InvalidOperationException();
        }

        var pixelFormat = _currentImageData.BitsPerPixel == 1 ? ImagePixelFormat.BW1 : ImagePixelFormat.RGB24;
        _currentImage ??= _scanningContext.ImageContext.Create(
            _currentImageData.Width, _currentImageData.Height, pixelFormat);
        _currentImage.SetResolution((float) _currentImageData.XRes, (float) _currentImageData.YRes);

        _transferredPixels += memoryBuffer.Columns * (long) memoryBuffer.Rows;
        _transferredWidth = Math.Max(_transferredWidth, memoryBuffer.Columns + memoryBuffer.XOffset);
        _transferredHeight = Math.Max(_transferredHeight, memoryBuffer.Rows + memoryBuffer.YOffset);

        // In case the real image dimensions don't match the specified image dimensions, we may need to get more memory.
        // The image will be realloc'd to the real size once we're done and know what that is.
        if (_transferredWidth > _currentImage.Width)
        {
            ReallocImage(Math.Max(_currentImage.Width * 2, _transferredWidth), _currentImage.Height);
        }
        if (_transferredHeight > _currentImage.Height)
        {
            ReallocImage(_currentImage.Width, Math.Max(_currentImage.Height * 2, _transferredHeight));
        }

        TwainMemoryBufferReader.CopyBufferToImage(memoryBuffer, _currentImageData, _currentImage);
        _progressEstimator.MarkProgress(Math.Min(_transferredPixels, _totalPixels), _totalPixels);
    }

    private void ReallocImage(int width, int height)
    {
        Debug.WriteLine($"NAPS2.TW - Realloc image {_currentImage!.Width}x{_currentImage.Height} -> {width}x{height}");
        var copy = _scanningContext.ImageContext.Create(width, height, _currentImage!.PixelFormat);
        new CopyBitwiseImageOp
        {
            Columns = Math.Min(width, _currentImage.Width),
            Rows = Math.Min(height, _currentImage.Height)
        }.Perform(_currentImage, copy);
        _currentImage.Dispose();
        _currentImage = copy;
    }

    public void Flush()
    {
        if (_currentImage != null && _transferredWidth > 0 && _transferredHeight > 0)
        {
            if (_transferredWidth != _currentImage.Width || _transferredHeight != _currentImage.Height)
            {
                // The real image dimensions don't match the specified image dimensions, so we have to realloc.
                ReallocImage(_transferredWidth, _transferredHeight);
            }
            _progressEstimator.MarkCompletion();
            _callback(_currentImage);
            _currentImage = null;
        }
    }

    public void Dispose()
    {
        if (_currentImage != null && _transferredPixels == _totalPixels &&
            _transferredWidth == _currentImageData?.Width && _transferredHeight == _currentImageData?.Height)
        {
            // If we have an error after a successful scan (so Flush isn't called normally) we still want to flush.
            // Obviously this won't work if the image dimensions are off (as we can't tell if the scan is complete or
            // not) but that should be a rare case.
            Flush();
        }
        _currentImage?.Dispose();
    }
}
#endif