using System.Reflection;
using System.Threading;
using NAPS2.Platform.Windows;
using NAPS2.Remoting.Worker;
using NTwain;
using NTwain.Data;

namespace NAPS2.Scan.Internal.Twain;

internal class TwainScanDriver : IScanDriver
{
    public static readonly TWIdentity TwainAppId =
        TWIdentity.CreateFromAssembly(DataGroups.Image | DataGroups.Control, Assembly.GetEntryAssembly());

    static TwainScanDriver()
    {
        // Path to the folder containing the 64-bit twaindsm.dll relative to NAPS2.Core.dll
        if (PlatformCompat.System.CanUseWin32)
        {
            string libDir = Environment.Is64BitProcess ? "_win64" : "_win32";
            var location = Assembly.GetExecutingAssembly().Location;
            var coreDllDir = System.IO.Path.GetDirectoryName(location);
            if (coreDllDir != null)
            {
                Win32.SetDllDirectory(System.IO.Path.Combine(coreDllDir, libDir));
            }
        }
#if DEBUG
        PlatformInfo.Current.Log.IsDebugEnabled = true;
#endif
    }

    private readonly ScanningContext _scanningContext;

    public TwainScanDriver(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        return Task.Run(() =>
        {
            var deviceList = InternalGetDeviceList(options);
            if (options.TwainOptions.Dsm != TwainDsm.Old && deviceList.Count == 0)
            {
                // Fall back to OldDsm in case of no devices
                // This is primarily for Citrix support, which requires using twain_32.dll for TWAIN passthrough
                deviceList = InternalGetDeviceList(options);
            }

            return deviceList;
        });
    }

    private static List<ScanDevice> InternalGetDeviceList(ScanOptions options)
    {
        PlatformInfo.Current.PreferNewDSM = options.TwainOptions.Dsm != TwainDsm.Old;
        var session = new TwainSession(TwainAppId);
        session.Open();
        try
        {
            return session.GetSources().Select(ds => new ScanDevice(ds.Name, ds.Name)).ToList();
        }
        finally
        {
            try
            {
                session.Close();
            }
            catch (Exception e)
            {
                Log.ErrorException("Error closing TWAIN session", e);
            }
        }
    }

    public async Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<IMemoryImage> callback)
    {
        var controller = GetTwainController(options);
        using var state = new TwainState(_scanningContext, scanEvents, callback);
        await controller.StartScan(options, state, cancelToken);
    }

    private ITwainController GetTwainController(ScanOptions options)
    {
        if (options.TwainOptions.Dsm != TwainDsm.NewX64 && Environment.Is64BitProcess)
        {
            return new RemoteTwainController(_scanningContext);
        }
        return new LocalTwainController();
    }

    private class TwainState : ITwainEvents, IDisposable
    {
        private readonly ScanningContext _scanningContext;
        private readonly IScanEvents _scanEvents;
        private readonly Action<IMemoryImage> _callback;
        private TwainImageData? _currentImageData;
        private IMemoryImage? _currentMemoryImage;
        private long _transferredPixels;
        private long _totalPixels;

        public TwainState(ScanningContext scanningContext, IScanEvents scanEvents, Action<IMemoryImage> callback)
        {
            _scanningContext = scanningContext;
            _scanEvents = scanEvents;
            _callback = callback;
        }

        public void PageStart(TwainPageStart pageStart)
        {
            _currentImageData = pageStart.ImageData;
            _currentMemoryImage?.Dispose();
            _currentMemoryImage = null;
            _transferredPixels = 0;
            _totalPixels = _currentImageData == null ? 0 : _currentImageData.Width * (long) _currentImageData.Height;
            _scanEvents.PageStart();
        }

        public void NativeImageTransferred(TwainNativeImage nativeImage)
        {
            using var image = _scanningContext.ImageContext.Load(new MemoryStream(nativeImage.Buffer.ToByteArray()));
            _callback(image);
        }

        public void MemoryBufferTransferred(TwainMemoryBuffer memoryBuffer)
        {
            if (_currentImageData == null)
            {
                throw new InvalidOperationException();
            }

            // TODO: Verify samples etc.
            // TODO: Also support grayscale
            var pixelFormat = _currentImageData.BitsPerPixel == 1 ? ImagePixelFormat.BW1 :
                _currentImageData.BitsPerPixel == 24 ? ImagePixelFormat.RGB24 :
                throw new InvalidOperationException($"Unsupported bits per pixel: {_currentImageData.BitsPerPixel}");
            _currentMemoryImage ??= _scanningContext.ImageContext.Create(
                _currentImageData.Width, _currentImageData.Height, pixelFormat);

            _transferredPixels += memoryBuffer.Columns * (long) memoryBuffer.Rows;
            _scanEvents.PageProgress(_transferredPixels / (double) _totalPixels);

            var bufferReader = new TwainMemoryBufferReader();
            bufferReader.ReadBuffer(memoryBuffer, pixelFormat, _currentMemoryImage);

            if (_transferredPixels == _totalPixels)
            {
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
}