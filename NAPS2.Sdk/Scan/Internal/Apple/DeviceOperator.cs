#if MAC
using System.Threading;
using CoreGraphics;
using Foundation;
using ImageCaptureCore;
using NAPS2.Images.Bitwise;
using NAPS2.Scan.Exceptions;

namespace NAPS2.Scan.Internal.Apple;

internal class DeviceOperator : ICScannerDeviceDelegate
{
    private readonly ScanningContext _scanningContext;
    private readonly ICScannerDevice _device;
    private readonly ScanOptions _options;
    private readonly IScanEvents _scanEvents;
    private readonly Action<IMemoryImage> _callback;
    private readonly TaskCompletionSource _openSessionTcs = new();
    private readonly TaskCompletionSource _readyTcs = new();
    private TaskCompletionSource<ICScannerFunctionalUnit> _unitTcs = new();
    private readonly TaskCompletionSource _scanTcs = new();
    private readonly TaskCompletionSource _closeTcs = new();
    private MemoryStream? _buffer;

    public DeviceOperator(ScanningContext scanningContext, ICScannerDevice device, ScanOptions options,
        CancellationToken cancelToken, IScanEvents scanEvents, Action<IMemoryImage> callback)
    {
        _scanningContext = scanningContext;
        _device = device;
        _options = options;
        _scanEvents = scanEvents;
        _callback = callback;

        cancelToken.Register(() =>
        {
            _openSessionTcs.TrySetCanceled();
            _readyTcs.TrySetCanceled();
            _unitTcs.TrySetCanceled();
            _scanTcs.TrySetCanceled();
            _closeTcs.TrySetCanceled();
        });
    }

    public override void DidOpenSession(ICDevice device, NSError? error)
    {
        SetResultOrError(_openSessionTcs, error);
    }

    public override void DidBecomeReady(ICDevice device)
    {
        _readyTcs.TrySetResult();
    }

    public override void DidCloseSession(ICDevice device, NSError? error)
    {
        SetResultOrError(_closeTcs, error);
    }

    public override void DidReceiveStatusInformation(ICDevice device, NSDictionary<NSString, NSObject> status)
    {
        var state = status[ICStatusNotificationKeys.NotificationKey] as NSString;
        Console.WriteLine($"{nameof(DidReceiveStatusInformation)}: Status: {status} State {state}");

        if (state == ICScannerStatus.WarmingUp)
        {
            _scanEvents.PageStart();
        }
    }

    public override void DidEncounterError(ICDevice device, NSError? error)
    {
        var ex = error != null ? new DeviceException(error.Description) : new DeviceException();
        // TODO: Put these in a list or something
        _openSessionTcs.TrySetException(ex);
        _readyTcs.TrySetException(ex);
        _unitTcs.TrySetException(ex);
        _scanTcs.TrySetException(ex);
        _closeTcs.TrySetException(ex);
    }

    // TODO: This will be called if the scanner is in use. We can consider waiting a couple seconds for the scanner
    // TODO: to become available before sending a busy error.
    public override void DidBecomeAvailable(ICScannerDevice scanner)
    {
        Console.WriteLine($"{nameof(DidBecomeAvailable)}: {scanner}");
    }

    public override void DidSelectFunctionalUnit(
        ICScannerDevice scanner, ICScannerFunctionalUnit functionalUnit, NSError? error)
    {
        SetResultOrError(_unitTcs, functionalUnit, error);
    }

    public override void DidScanToBandData(ICScannerDevice scanner, ICScannerBandData data)
    {
        var (pixelFormat, subPixelType) = (data.PixelDataType, data.NumComponents, data.BitsPerComponent) switch
        {
            (ICScannerPixelDataType.BW, 1, 1) => (ImagePixelFormat.BW1, SubPixelType.Bit),
            (ICScannerPixelDataType.Gray, 1, 8) => (ImagePixelFormat.Gray8, SubPixelType.Gray),
            (ICScannerPixelDataType.Rgb, 3, 8) => (ImagePixelFormat.RGB24, SubPixelType.Rgb),
            (ICScannerPixelDataType.Rgb, 4, 8) => (ImagePixelFormat.ARGB32, SubPixelType.Rgba),
            _ => (ImagePixelFormat.Unsupported, null)
        };
        if (pixelFormat == ImagePixelFormat.Unsupported)
        {
            // TODO: Set errors
            return;
        }
        var bufferInfo = new PixelInfo(
            (int) data.FullImageWidth,
            (int) data.FullImageHeight,
            subPixelType!,
            (int) data.BytesPerRow);
        _buffer ??= new MemoryStream((int) bufferInfo.Length);
        data.DataBuffer!.AsStream().CopyTo(_buffer);
        // TODO: The buffer gets written pretty much all at once, at least for escl - maybe we can/should reuse TwainProgressEstimator
        _scanEvents.PageProgress(_buffer.Length / (double) bufferInfo.Length);
        if (_buffer.Length >= bufferInfo.Length)
        {
            var image = _scanningContext.ImageContext.Create(
                (int) data.FullImageWidth, (int) data.FullImageHeight, pixelFormat);
            new CopyBitwiseImageOp().Perform(_buffer.GetBuffer(), bufferInfo, image);
            _callback(image);
            _buffer = null;
        }
    }

    public override void DidCompleteScan(ICScannerDevice scanner, NSError? error)
    {
        SetResultOrError(_scanTcs, error);
    }

    private void SetResultOrError(TaskCompletionSource tcs, NSError? error)
    {
        if (error != null)
        {
            tcs.TrySetException(GetException(error));
        }
        else
        {
            tcs.TrySetResult();
        }
    }

    private void SetResultOrError<T>(TaskCompletionSource<T> tcs, T value, NSError? error)
    {
        if (error != null)
        {
            tcs.TrySetException(GetException(error));
        }
        else
        {
            tcs.TrySetResult(value);
        }
    }

    private Exception GetException(NSError error)
    {
        return new DeviceException(error.LocalizedDescription);
    }

    public override void DidRemoveDevice(ICDevice device)
    {
    }

    public async Task Scan()
    {
        try
        {
            _device.Delegate = this;
            _device.RequestOpenSession();
            await _openSessionTcs.Task;
            await _readyTcs.Task;
            var unit = await SelectUnit(_options.PaperSource == PaperSource.Flatbed
                ? ICScannerFunctionalUnitType.Flatbed
                : ICScannerFunctionalUnitType.DocumentFeeder);
            SetScanArea(unit);
            // TODO: Check supported resolutions?
            unit.Resolution = (nuint) _options.Dpi;
            unit.BitDepth = _options.BitDepth == BitDepth.BlackAndWhite
                ? ICScannerBitDepth.Bits1
                : ICScannerBitDepth.Bits8;
            unit.PixelDataType = _options.BitDepth switch
            {
                BitDepth.BlackAndWhite => ICScannerPixelDataType.BW,
                BitDepth.Grayscale => ICScannerPixelDataType.Gray,
                _ => ICScannerPixelDataType.Rgb
            };
            _device.TransferMode = ICScannerTransferMode.MemoryBased;
            // TODO: increase? or maybe not as this could still be useful progress for twain scanners
            _device.MaxMemoryBandSize = 65536;
            _device.RequestScan();
            await _scanTcs.Task;
            _device.RequestCloseSession();
            await _closeTcs.Task;
        }
        catch (TaskCanceledException)
        {
            // TODO: Cancellation not working
            _device.CancelScan();
        }
        finally
        {
            if (_device.HasOpenSession)
            {
                _device.RequestCloseSession();
            }
        }
    }

    private void SetScanArea(ICScannerFunctionalUnit unit)
    {
        unit.MeasurementUnit = ICScannerMeasurementUnit.Inches;
        var maxSize = unit.PhysicalSize;
        var width = Math.Min((double) _options.PageSize!.WidthInInches, maxSize.Width);
        var height = Math.Min((double) _options.PageSize.HeightInInches, maxSize.Height);
        var deltaX = maxSize.Width - width;
        var offsetX = _options.PageAlign switch
        {
            HorizontalAlign.Left => deltaX,
            HorizontalAlign.Center => deltaX / 2,
            _ => 0
        };
        unit.ScanArea = new CGRect(offsetX, 0, offsetX + width, height);
    }

    private async Task<ICScannerFunctionalUnit> SelectUnit(ICScannerFunctionalUnitType unitType)
    {
        // TODO: Can we clean this up at all?
        var availableUnits = _device.AvailableFunctionalUnitTypes.Select(x =>
            (ICScannerFunctionalUnitType) (int) x).ToList();
        await _unitTcs.Task;
        _unitTcs = new TaskCompletionSource<ICScannerFunctionalUnit>();
        if (availableUnits.Contains(unitType))
        {
            _device.RequestSelectFunctionalUnit(unitType);
            var result = await _unitTcs.Task;
            return result;
        }
        return _device.SelectedFunctionalUnit;
    }
}
#endif