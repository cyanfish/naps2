#if MAC
using System.Collections.Immutable;
using System.Threading;
using AppKit;
using CoreGraphics;
using Foundation;
using ImageCaptureCore;
using Microsoft.Extensions.Logging;
using NAPS2.Images.Bitwise;
using NAPS2.Images.Mac;
using NAPS2.Scan.Exceptions;

namespace NAPS2.Scan.Internal.Apple;

internal class DeviceOperator : ICScannerDeviceDelegate
{
    private readonly ScanningContext _scanningContext;
    private readonly ILogger _logger;
    private readonly ICScannerDevice _device;
    private ICScannerFunctionalUnit? _unit;
    private nuint _resolution;
    private readonly DeviceReader _reader;
    private readonly ScanOptions _options;
    private readonly IScanEvents _scanEvents;
    private readonly Action<IMemoryImage> _callback;
    private readonly TaskCompletionSource _openSessionTcs = new();
    private readonly TaskCompletionSource _readyTcs = new();
    private TaskCompletionSource<ICScannerFunctionalUnit> _unitTcs = new();
    private readonly TaskCompletionSource _scanSuccessTcs = new();
    private readonly TaskCompletionSource _scanCompleteTcs = new();
    private TaskCompletionSource? _cancelTcs;
    private readonly TaskCompletionSource _closeTcs = new();
    private readonly List<Task> _copyTasks = new();
    private MemoryStream? _buffer;

    public DeviceOperator(ScanningContext scanningContext, ICScannerDevice device, DeviceReader reader,
        ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IMemoryImage> callback)
    {
        _scanningContext = scanningContext;
        _logger = scanningContext.Logger;
        _device = device;
        _reader = reader;
        _options = options;
        _scanEvents = scanEvents;
        _callback = callback;

        cancelToken.Register(() =>
        {
            _openSessionTcs.TrySetCanceled();
            _readyTcs.TrySetCanceled();
            _unitTcs.TrySetCanceled();
            _scanSuccessTcs.TrySetCanceled();
            _closeTcs.TrySetCanceled();
        });
    }

    public override void DidOpenSession(ICDevice device, NSError? error)
    {
        _logger.LogDebug("DidOpenSession {Error}", error);
        SetResultOrError(_openSessionTcs, error);
    }

    public override void DidBecomeReady(ICDevice device)
    {
        _logger.LogDebug("DidBecomeReady");
        _readyTcs.TrySetResult();
    }

    public override void DidCloseSession(ICDevice device, NSError? error)
    {
        _logger.LogDebug("DidCloseSession {Error}", error);
        SetResultOrError(_closeTcs, error);
    }

    public override void DidReceiveStatusInformation(ICDevice device, NSDictionary<NSString, NSObject> status)
    {
        var state = status[ICStatusNotificationKeys.NotificationKey] as NSString;
        _logger.LogDebug("DidReceiveStatusInformation {State}", state);

        if (state == ICScannerStatus.WarmingUp)
        {
            _scanEvents.PageStart();
        }
        if (_cancelTcs != null && !_unit!.ScanInProgress)
        {
            _cancelTcs.SetResult();
        }
    }

    public override void DidEncounterError(ICDevice device, NSError? error)
    {
        _logger.LogDebug("DidEncounterError {Error}", error);
        var ex = error != null ? new DeviceException(error.Description) : new DeviceException();
        // TODO: Put these in a list or something
        _openSessionTcs.TrySetException(ex);
        _readyTcs.TrySetException(ex);
        _unitTcs.TrySetException(ex);
        _scanSuccessTcs.TrySetException(ex);
        _closeTcs.TrySetException(ex);
    }

    // TODO: This will be called if the scanner is in use. We can consider waiting a couple seconds for the scanner
    // TODO: to become available before sending a busy error.
    public override void DidBecomeAvailable(ICScannerDevice scanner)
    {
        _logger.LogDebug("DidBecomeAvailable");
    }

    public override void DidSelectFunctionalUnit(
        ICScannerDevice scanner, ICScannerFunctionalUnit functionalUnit, NSError? error)
    {
        _logger.LogDebug("DidSelectFunctionalUnit {Unit} {Error}", functionalUnit.GetType().Name, error);
        SetResultOrError(_unitTcs, functionalUnit, error);
    }

    public override void DidScanToBandData(ICScannerDevice scanner, ICScannerBandData data)
    {
        var expectedBufferLength = (int) (data.FullImageHeight * data.BytesPerRow);
        _buffer ??= new MemoryStream(expectedBufferLength);
        data.DataBuffer!.AsStream().CopyTo(_buffer);
        // TODO: The buffer gets written pretty much all at once, at least for escl - maybe we can/should reuse TwainProgressEstimator
        _scanEvents.PageProgress(_buffer.Length / (double) expectedBufferLength);

        if (_buffer.Length >= expectedBufferLength)
        {
            _logger.LogDebug("DidScanToBandData buffer complete");
            var fullBuffer = _buffer;
            _buffer = null;
            var copyTask = Task.Run(() =>
            {
                // We prefer to use the provided color profile for maximum color accuracy. If one isn't present we fall
                // back to a direct bitwise copy if it's in a supported pixel format.
                var (pixelFormat, subPixelType) = (data.PixelDataType, data.NumComponents, data.BitsPerComponent) switch
                {
                    (ICScannerPixelDataType.BW, 1, 1) => (ImagePixelFormat.BW1, SubPixelType.Bit),
                    (ICScannerPixelDataType.Gray, 1, 8) => (ImagePixelFormat.Gray8, SubPixelType.Gray),
                    (ICScannerPixelDataType.Rgb, 3, 8) => (ImagePixelFormat.RGB24, SubPixelType.Rgb),
                    (ICScannerPixelDataType.Rgb, 4, 8) => (ImagePixelFormat.RGB24, SubPixelType.Rgbn),
                    _ => (ImagePixelFormat.Unknown, null)
                };
                _logger.LogDebug(
                    "Image data: width {Width}, height {Height}, type {Type}, comp {Comp}, " +
                    "bits/comp {BitsPerComp}, bits/pixel {BitsPerPixel}, bytes/row {BytesPerRow}, data len {DataLen}",
                    data.FullImageWidth, data.FullImageHeight, data.PixelDataType, data.NumComponents,
                    data.BitsPerComponent, data.BitsPerPixel, data.BytesPerRow, fullBuffer.Length);
                if (data.ColorSyncProfilePath != null)
                {
                    _logger.LogDebug($"Flushing image with color sync profile {data.ColorSyncProfilePath}");
                    FlushImageWithColorSpace(fullBuffer, data, subPixelType);
                }
                else if (pixelFormat != ImagePixelFormat.Unknown && subPixelType != null)
                {
                    _logger.LogDebug($"Flushing image with pixel format {pixelFormat}");
                    FlushImageDirectly(fullBuffer, data, subPixelType, pixelFormat);
                }
                else
                {
                    _logger.LogError(
                        "No color sync profile and unsupported ICC pixel format " +
                        "{PixelDataType} {NumComponents} {BitsPerComponent}",
                        data.PixelDataType, data.NumComponents, data.BitsPerComponent);
                }
                _scanEvents.PageStart();
            });
            lock (this)
            {
                _copyTasks.Add(copyTask);
            }
        }
    }

    private void FlushImageDirectly(MemoryStream fullBuffer, ICScannerBandData data, SubPixelType subPixelType,
        ImagePixelFormat pixelFormat)
    {
        var image = _scanningContext.ImageContext.Create(
            (int) data.FullImageWidth, (int) data.FullImageHeight, pixelFormat);
        var bufferInfo = new PixelInfo(
            (int) data.FullImageWidth,
            (int) data.FullImageHeight,
            subPixelType!,
            (int) data.BytesPerRow);
        _buffer ??= new MemoryStream((int) bufferInfo.Length);
        new CopyBitwiseImageOp().Perform(fullBuffer.GetBuffer(), bufferInfo, image);
        _logger.LogDebug("Setting resolution to {Dpi}", _resolution);
        image.SetResolution(_resolution, _resolution);
        _callback(image);
    }

    private void FlushImageWithColorSpace(MemoryStream fullBuffer, ICScannerBandData data, SubPixelType? subPixelType)
    {
        var colorSpace = CGColorSpace.CreateIccData(NSData.FromFile(data.ColorSyncProfilePath!));
        var w = (int) data.FullImageWidth;
        var h = (int) data.FullImageHeight;
        var bitsPerComponent = (int) data.BitsPerComponent;
        var bitsPerPixel = (int) data.BitsPerPixel;
        var bytesPerRow = (int) data.BytesPerRow;
        var flags = (bitsPerPixel == 32 ? CGBitmapFlags.NoneSkipLast : CGBitmapFlags.None) |
                    CGBitmapFlags.ByteOrderDefault;
        var buffer = fullBuffer.GetBuffer();
        var dataProvider = new CGDataProvider(buffer, 0, buffer.Length);

        // There is an apparent bug in ImageCaptureCore where grayscale images can report a bytesPerRow value that is
        // aligned to a word boundary (and the buffer is sized to match), but the actual data is stored as if that
        // wasn't the case, leaving a block of zeros at the end of the buffer. We correct for this here.
        // TODO: Is there any case where this will backfire? Can we detect the problem (e.g. by checking for zeros at
        // the end of the buffer)?
        if (subPixelType == SubPixelType.Gray)
        {
            bytesPerRow = w;
        }

        var cgImage = new CGImage(w, h, bitsPerComponent, bitsPerPixel, bytesPerRow, colorSpace, flags,
            dataProvider, null, true, CGColorRenderingIntent.Default);
        var imageRep = new NSBitmapImageRep(cgImage);
        var nsImage = new NSImage();
        nsImage.AddRepresentation(imageRep);
        // TODO: Could maybe do this without the NAPS2.Images.Mac reference but that would require duplicating
        // a bunch of logic to normalize image reps etc.
        var macImage = new MacImage(nsImage);
        _logger.LogDebug("Setting resolution to {Dpi}", _resolution);
        macImage.SetResolution(_resolution, _resolution);
        if (_scanningContext.ImageContext is MacImageContext)
        {
            _callback(macImage);
        }
        else
        {
            var image = macImage.Copy(_scanningContext.ImageContext);
            macImage.Dispose();
            _callback(image);
        }
    }

    public override void DidCompleteScan(ICScannerDevice scanner, NSError? error)
    {
        _logger.LogDebug("DidCompleteScan {Error}", error);
        SetResultOrError(_scanSuccessTcs, error);
        SetResultOrError(_scanCompleteTcs, error);
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

    public async Task<ScanCaps> GetCaps()
    {
        try
        {
            _device.Delegate = this;
            _logger.LogDebug("ICC: Opening session for caps");
            _device.RequestOpenSession();
            await _openSessionTcs.Task;
            _logger.LogDebug("ICC: Waiting for ready");
            await _readyTcs.Task;

            var unitTypes = _device.AvailableFunctionalUnitTypes;
            bool supportsFeeder = unitTypes.Contains((NSNumber) (int) ICScannerFunctionalUnitType.Flatbed);
            bool supportsFlatbed = unitTypes.Contains((NSNumber) (int) ICScannerFunctionalUnitType.DocumentFeeder);
            bool supportsDuplex = false;
            PerSourceCaps? flatbedCaps = null;
            PerSourceCaps? feederCaps = null;
            PerSourceCaps? duplexCaps = null;

            _logger.LogDebug("ICC: Selecting flatbed unit");
            _unit = await SelectUnit(ICScannerFunctionalUnitType.Flatbed);
            if (_unit is ICScannerFunctionalUnitFlatbed flatbedUnit)
            {
                flatbedCaps = GetCapsFromUnit(flatbedUnit);
            }

            _logger.LogDebug("ICC: Selecting feeder unit");
            _unit = await SelectUnit(ICScannerFunctionalUnitType.DocumentFeeder);
            if (_unit is ICScannerFunctionalUnitDocumentFeeder feederUnit)
            {
                supportsDuplex = feederUnit.SupportsDuplexScanning;
                feederCaps = GetCapsFromUnit(feederUnit);
                if (supportsDuplex) duplexCaps = feederCaps;
            }

            _logger.LogDebug("ICC: Closing session");
            _device.RequestCloseSession();
            await _closeTcs.Task;
            _logger.LogDebug("ICC: Caps query success");

            return new ScanCaps
            {
                MetadataCaps = new()
                {
                    SerialNumber = _device.SerialNumber
                },
                PaperSourceCaps = new()
                {
                    SupportsFlatbed = supportsFlatbed,
                    SupportsFeeder = supportsFeeder,
                    SupportsDuplex = supportsDuplex,
                    CanCheckIfFeederHasPaper = true
                },
                FlatbedCaps = flatbedCaps,
                FeederCaps = feederCaps,
                DuplexCaps = duplexCaps
            };
        }
        finally
        {
            if (_device.HasOpenSession)
            {
                _logger.LogDebug("ICC: Closing session (in finally)");
                _device.RequestCloseSession();
            }
        }
    }

    private PerSourceCaps GetCapsFromUnit(ICScannerFunctionalUnit unit)
    {
        unit.MeasurementUnit = ICScannerMeasurementUnit.Inches;
        return new PerSourceCaps
        {
            DpiCaps = new()
            {
                Values = unit.SupportedResolutions.Select(x => (int) x).Order().ToImmutableList()
            },
            BitDepthCaps = new()
            {
                SupportsBlackAndWhite = unit.SupportedBitDepths.Contains((nuint) ICScannerBitDepth.Bits1),
                SupportsGrayscale = unit.SupportedBitDepths.Contains((nuint) ICScannerBitDepth.Bits8),
                SupportsColor = unit.SupportedBitDepths.Contains((nuint) ICScannerBitDepth.Bits8)
            },
            PageSizeCaps = new()
            {
                ScanArea = new PageSize(
                    (decimal) unit.PhysicalSize.Width,
                    (decimal) unit.PhysicalSize.Height,
                    PageSizeUnit.Inch)
            }
        };
    }

    public async Task Scan()
    {
        try
        {
            _device.Delegate = this;
            _logger.LogDebug("ICC: Opening session");
            _device.RequestOpenSession();
            await _openSessionTcs.Task;
            _logger.LogDebug("ICC: Waiting for ready");
            await _readyTcs.Task;
            _logger.LogDebug("ICC: Selecting unit");
            _unit = await SelectUnit(_options.PaperSource is PaperSource.Flatbed or PaperSource.Auto
                ? ICScannerFunctionalUnitType.Flatbed
                : ICScannerFunctionalUnitType.DocumentFeeder);
            if (_unit is ICScannerFunctionalUnitDocumentFeeder { SupportsDuplexScanning: true } feederUnit)
            {
                feederUnit.DuplexScanningEnabled = _options.PaperSource == PaperSource.Duplex;
            }
            _logger.LogDebug("ICC: Setting scan parameters");
            SetScanArea(_unit);
            _resolution = GetClosestResolution((nuint) _options.Dpi, _unit);
            _unit.Resolution = _resolution;
            _unit.BitDepth = _options.BitDepth == BitDepth.BlackAndWhite
                ? ICScannerBitDepth.Bits1
                : ICScannerBitDepth.Bits8;
            _unit.PixelDataType = _options.BitDepth switch
            {
                BitDepth.BlackAndWhite => ICScannerPixelDataType.BW,
                BitDepth.Grayscale => ICScannerPixelDataType.Gray,
                _ => ICScannerPixelDataType.Rgb
            };
            _device.TransferMode = ICScannerTransferMode.MemoryBased;
            _device.MaxMemoryBandSize = 65536;
            _logger.LogDebug("ICC: Requesting scan");
            _device.RequestScan();
            await _scanSuccessTcs.Task;
            Task[] copyTasks;
            lock (this)
            {
                copyTasks = _copyTasks.ToArray();
            }
            if (copyTasks.Length == 0 && _unit is ICScannerFunctionalUnitDocumentFeeder { DocumentLoaded: false })
            {
                _logger.LogDebug("ICC: No pages in feeder");
                throw new DeviceFeederEmptyException();
            }
            _logger.LogDebug("ICC: Waiting for scan results");
            await Task.WhenAll(copyTasks);
            _logger.LogDebug("ICC: Closing session");
            _device.RequestCloseSession();
            await _closeTcs.Task;
            _logger.LogDebug("ICC: Scan success");
        }
        catch (TaskCanceledException)
        {
            if (_unit != null && _unit.ScanInProgress)
            {
                _cancelTcs = new TaskCompletionSource();
                _logger.LogDebug("ICC: Cancelling scan");
                _device.CancelScan();
                await Task.WhenAny(_scanCompleteTcs.Task, _cancelTcs.Task);
            }
            _logger.LogDebug("ICC: Scan cancelled");
        }
        finally
        {
            if (_device.HasOpenSession)
            {
                _logger.LogDebug("ICC: Closing session (in finally)");
                _device.RequestCloseSession();
            }
        }
    }

    private ICScannerDocumentType GetDocumentTypeFromPageSize(PageSize? pageSize)
    {
        // TODO: Maybe some tolerance, e.g. if translating over EsclScanServer?
        if (pageSize == PageSize.A3) return ICScannerDocumentType.A3;
        if (pageSize == PageSize.A4) return ICScannerDocumentType.A4;
        if (pageSize == PageSize.A5) return ICScannerDocumentType.A5;
        if (pageSize == PageSize.Letter) return ICScannerDocumentType.USLetter;
        if (pageSize == PageSize.Legal) return ICScannerDocumentType.USLegal;
        if (pageSize == PageSize.B4) return ICScannerDocumentType.IsoB4;
        if (pageSize == PageSize.B5) return ICScannerDocumentType.IsoB5;
        return ICScannerDocumentType.Default;
    }

    private nuint GetClosestResolution(nuint dpi, ICScannerFunctionalUnit unit)
    {
        var targetDpi = dpi;
        if (unit.SupportedResolutions.Count > 0)
        {
            targetDpi = unit.SupportedResolutions.MinBy(x => Math.Abs((int) (x - dpi)));
        }
        if (targetDpi != dpi)
        {
            _logger.LogDebug("ICC: Correcting resolution from {InDpi} to {OutDpi}", dpi, targetDpi);
        }
        return targetDpi;
    }

    private void SetScanArea(ICScannerFunctionalUnit unit)
    {
        // Setting DocumentType should be redundant (setting ScanArea is more general), but it shouldn't hurt and may
        // help with some issues with particular scanners.
        if (_unit is ICScannerFunctionalUnitDocumentFeeder feederUnit)
        {
            feederUnit.DocumentType = GetDocumentTypeFromPageSize(_options.PageSize);
        }
        if (_unit is ICScannerFunctionalUnitFlatbed flatbedUnit)
        {
            flatbedUnit.DocumentType = GetDocumentTypeFromPageSize(_options.PageSize);
        }
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _device.Delegate = null;
        }
        base.Dispose(disposing);
    }
}
#endif