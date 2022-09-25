#if !MAC
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
    private IMemoryImage? _currentMemoryImage;
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
        _currentImageData = pageStart.ImageData;
        _currentMemoryImage?.Dispose();
        _currentMemoryImage = null;
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
        _currentMemoryImage ??= _scanningContext.ImageContext.Create(
            _currentImageData.Width, _currentImageData.Height, pixelFormat);
        _currentMemoryImage.SetResolution((float) _currentImageData.XRes, (float) _currentImageData.YRes);

        _transferredPixels += memoryBuffer.Columns * (long) memoryBuffer.Rows;

        TwainMemoryBufferReader.CopyBufferToImage(memoryBuffer, _currentImageData, _currentMemoryImage);
        _progressEstimator.MarkProgress(_transferredPixels, _totalPixels);

        if (_transferredPixels == _totalPixels)
        {
            _progressEstimator.MarkCompletion();
            // TODO: Throw an error if there's a pixel mismatch, i.e. we go to the next page / finish with too few, or have too many
            _callback(_currentMemoryImage);
            _currentMemoryImage = null;
        }
    }

    public void Dispose()
    {
        _currentMemoryImage?.Dispose();
    }
}
#endif