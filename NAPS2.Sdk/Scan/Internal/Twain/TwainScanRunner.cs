#if !MAC
using System.Threading;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using NAPS2.Remoting.Worker;
using NAPS2.Scan.Exceptions;
using NTwain;
using NTwain.Data;

namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// Interfaces with the native NTwain TwainSession to perform an actual scan. The raw scanned data is propagated via the
/// ITwainEvents interface. This logic involves quite a bit of complicated state management related to the Twain spec.
/// https://twain.org/wp-content/uploads/2015/05/TWAIN-2.3-Specification.pdf
/// </summary>
internal class TwainScanRunner
{
    private readonly ILogger _logger;
    private readonly TwainDsm _dsm;
    private readonly ScanOptions _options;
    private readonly CancellationToken _cancelToken;
    private readonly ITwainEvents _twainEvents;
    private readonly TwainHandleManager _handleManager;
    private readonly TwainSession _session;
    private readonly TaskCompletionSource<bool> _tcs;
    private readonly TaskCompletionSource<bool> _sourceDisabledTcs;
    private DataSource? _source;

    public TwainScanRunner(ILogger logger, TWIdentity twainAppId, TwainDsm dsm, ScanOptions options,
        CancellationToken cancelToken, ITwainEvents twainEvents)
    {
        _logger = logger;
        _dsm = dsm;
        _options = options;
        _cancelToken = cancelToken;
        _twainEvents = twainEvents;

        _handleManager = TwainHandleManager.Factory();
        PlatformInfo.Current.PreferNewDSM = dsm != TwainDsm.Old;
        _logger.LogDebug($"Using TWAIN DSM: {PlatformInfo.Current.ExpectedDsmPath}");
        _session = new TwainSession(twainAppId);
        _session.TransferReady += TransferReady;
        _session.DataTransferred += DataTransferred;
        _session.TransferCanceled += TransferCanceled;
        _session.TransferError += TransferError;
        _session.SourceDisabled += SourceDisabled;
        _session.StateChanged += StateChanged;
        _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _sourceDisabledTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public Task Run()
    {
        _handleManager.Invoker.InvokeDispatch(Init);
        return _tcs.Task;
    }

    private void Init()
    {
        try
        {
            _logger.LogDebug("NAPS2.TW - Opening session");
            bool useNativeUi = _options.UseNativeUI || _options.TwainOptions.ShowProgress;
            var rc = _session.Open(_handleManager.CreateMessageLoopHook(_options.DialogParent, useNativeUi));
            if (rc != ReturnCode.Success)
            {
                throw new DeviceException($"TWAIN session open error: {rc}");
            }

            _logger.LogDebug("NAPS2.TW - Finding source");
            _source = _session.FirstOrDefault(x => x.Name == _options.Device!.ID);
            if (_source == null)
            {
                throw new DeviceNotFoundException();
            }

            _logger.LogDebug("NAPS2.TW - Opening source");
            _logger.LogDebug(
                "NAPS2.TW - Name: {Name}; Manu: {Manu}; Family: {Family}; Version: {Version}; Protocol: {Protocol}",
                _source.Name, _source.Manufacturer, _source.ProductFamily, _source.Version, _source.ProtocolVersion);
            rc = _source.Open();
            if (rc != ReturnCode.Success)
            {
                throw GetExceptionForStatus(_session.GetStatus());
            }

            _logger.LogDebug("NAPS2.TW - Configuring source");
            ConfigureSource(_source);

            _logger.LogDebug("NAPS2.TW - Enabling source");
            var ui = _options.UseNativeUI ? SourceEnableMode.ShowUI : SourceEnableMode.NoUI;
            var enableHandle = _handleManager.GetEnableHandle(_options.DialogParent, useNativeUi);
            // Note that according to the twain spec, on Windows it is recommended to set the modal parameter to false
            rc = _source.Enable(ui, false, enableHandle);
            if (rc != ReturnCode.Success)
            {
                throw GetExceptionForStatus(_source.GetStatus());
            }

            _cancelToken.Register(() => _handleManager.Invoker.Invoke(FinishWithCancellation));
            _sourceDisabledTcs.Task.ContinueWith(_ => _handleManager.Invoker.Invoke(FinishWithCompletion)).AssertNoAwait();
        }
        catch (Exception ex)
        {
            FinishWithError(ex);
        }
    }

    private void FinishWithCancellation()
    {
        _logger.LogDebug("NAPS2.TW - Finishing with cancellation");
        if (_session.State != 5)
        {
            // If we're in state 6 or 7, this will abort the ongoing transfer via ForceStepDown.
            // If we're in state 4 or lower, then we're not transferring and this will just clean up the source/session.
            UnloadTwain();
            _tcs.TrySetResult(false);
        }
        else
        {
            // If we're in state 5, we can just wait for the TransferReady event and "naturally" cancel the transfer.
            // (Or if we're in state 5 in the process of finishing all transfers, then we don't need to cancel anyway.)
            // This will result in FinishWithCompletion being called when the source disables itself.
            // The alternative of calling ForceStepDown from state 5 seems to produce an error message from the scanner.
            _logger.LogDebug("NAPS2.TW - Will cancel via TransferReady");
        }
    }

    private void FinishWithError(Exception ex)
    {
        _logger.LogDebug(ex, "NAPS2.TW - Finishing with error");
        // If we're in state 5 or higher, we'll call ForceStepDown, which could potentially produce additional errors,
        // but what alternative is there?
        // If we're in state 4 or lower, this will just clean up the source/session.
        UnloadTwain();
        _tcs.TrySetException(ex);
    }

    private void FinishWithCompletion()
    {
        _logger.LogDebug("NAPS2.TW - Finishing with completion");
        // At this point we should be in state 4 and this will clean up the source/session.
        UnloadTwain();
        _tcs.TrySetResult(true);
    }

    private void UnloadTwain()
    {
        try
        {
            if (_session.State > 4)
            {
                // If a transfer is initialized or in progress, this will abort it and also clean up the source/session.
                _session.ForceStepDown(2);
                return;
            }
            // If a transfer isn't in progress, we just clean up the source/session as needed.
            if (_session.State == 4)
            {
                _source!.Close();
            }
            if (_session.State >= 3)
            {
                _session.Close();
            }
        }
        finally
        {
            _handleManager.Dispose();
        }
    }

    private void StateChanged(object? sender, EventArgs e)
    {
        _logger.LogDebug($"NAPS2.TW - StateChanged (to {_session.State})");
    }

    private void SourceDisabled(object? sender, EventArgs e)
    {
        _logger.LogDebug("NAPS2.TW - SourceDisabled");
        _sourceDisabledTcs.TrySetResult(true);
    }

    private void TransferCanceled(object? sender, TransferCanceledEventArgs e)
    {
        _logger.LogDebug("NAPS2.TW - TransferCanceled");
        _twainEvents.TransferCanceled(new TwainTransferCanceled());
    }

    private void TransferError(object? sender, TransferErrorEventArgs e)
    {
        _logger.LogDebug("NAPS2.TW - TransferError");
        FinishWithError(e.Exception ?? GetExceptionForStatus(e.SourceStatus));
    }

    private Exception GetExceptionForStatus(TWStatus status)
    {
        switch (status.ConditionCode)
        {
            case ConditionCode.OperationError:
                // This means the scanner has already shown the user an error message, so we don't need to show another.
                // TODO: The spec says that if CAP_INDICATORS is false with NO_UI, we should still display the error to
                // the user, but with my test scanners that seems unnecessary so for now I'm not showing the error
                // regardless.
                return new AlreadyHandledDriverException();
            case ConditionCode.PaperJam:
                return new DevicePaperJamException();
            case ConditionCode.CheckDeviceOnline when _session.State <= 3:
                return new DeviceOfflineException();
            case ConditionCode.CheckDeviceOnline when _session.State >= 4:
                return new DeviceCommunicationException();
            default:
                return new DeviceException($"TWAIN error: {status.ConditionCode}");
        }
    }

    private void DataTransferred(object? sender, DataTransferredEventArgs e)
    {
        _logger.LogDebug("NAPS2.TW - DataTransferred");
        try
        {
            // TODO: We probably want to support native transfer for net6
            if (_options.TwainOptions.TransferMode == TwainTransferMode.Memory && e.MemoryData == null)
            {
                _logger.LogDebug("NAPS2.TW - Expected memory transfer, but got native transfer?");
            }
#if NET6_0_OR_GREATER
            if (e.MemoryData == null)
            {
                _logger.LogError("NAPS2.TW - Native transfer is not yet supported with the net6 build.");
                return;
            }
            _twainEvents.MemoryBufferTransferred(ToMemoryBuffer(e.MemoryData, e.MemoryInfo));
#else
            if (e.MemoryData != null)
            {
                _twainEvents.MemoryBufferTransferred(ToMemoryBuffer(e.MemoryData, e.MemoryInfo));
            }
            else
            {
                _twainEvents.NativeImageTransferred(new TwainNativeImage
                {
                    Buffer = ByteString.FromStream(e.GetNativeImageStream())
                });
            }
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending TWAIN data transfer event");
        }
    }

    private void TransferReady(object? sender, TransferReadyEventArgs e)
    {
        _logger.LogDebug("NAPS2.TW - TransferReady");
        try
        {
            var pageStart = new TwainPageStart();
#if NET6_0_OR_GREATER
            pageStart.ImageData = ToImageData(e.PendingImageInfo);
#else
            if (_options.TwainOptions.TransferMode == TwainTransferMode.Memory)
            {
                pageStart.ImageData = ToImageData(e.PendingImageInfo);
            }
#endif
            _twainEvents.PageStart(pageStart);
            if (_cancelToken.IsCancellationRequested)
            {
                e.CancelAll = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending TWAIN transfer ready event");
        }
    }

    private TwainMemoryBuffer ToMemoryBuffer(byte[] buffer, TWImageMemXfer memInfo)
    {
        return new TwainMemoryBuffer
        {
            Buffer = ByteString.CopyFrom(buffer),
            Columns = (int) memInfo.Columns,
            Rows = (int) memInfo.Rows,
            XOffset = (int) memInfo.XOffset,
            YOffset = (int) memInfo.YOffset,
            BytesPerRow = (int) memInfo.BytesPerRow
        };
    }

    private static TwainImageData ToImageData(TWImageInfo imageInfo)
    {
        var imageData = new TwainImageData
        {
            Width = imageInfo.ImageWidth,
            Height = imageInfo.ImageLength,
            BitsPerPixel = imageInfo.BitsPerPixel,
            SamplesPerPixel = imageInfo.SamplesPerPixel,
            PixelType = (int) imageInfo.PixelType,
            XRes = imageInfo.XResolution,
            YRes = imageInfo.YResolution
        };
        imageData.BitsPerSample.AddRange(imageInfo.BitsPerSample.Select(x => (int) x));
        return imageData;
    }

    private void ConfigureSource(DataSource source)
    {
#if NET6_0_OR_GREATER
        source.Capabilities.ICapXferMech.SetValue(XferMech.Memory);
#else
        // Transfer Mode
        if (_options.TwainOptions.TransferMode == TwainTransferMode.Memory)
        {
            source.Capabilities.ICapXferMech.SetValue(XferMech.Memory);
        }
#endif

        if (_options.UseNativeUI)
        {
            return;
        }

        // Progress UI
        if (!_options.TwainOptions.ShowProgress)
        {
            source.Capabilities.CapIndicators.SetValue(BoolType.False);
        }

        // Paper Source
        switch (_options.PaperSource)
        {
            case PaperSource.Auto: // Assume the data source will ignore if unsupported
            case PaperSource.Flatbed:
                source.Capabilities.CapFeederEnabled.SetValue(BoolType.False);
                source.Capabilities.CapDuplexEnabled.SetValue(BoolType.False);
                break;
            case PaperSource.Feeder:
                source.Capabilities.CapFeederEnabled.SetValue(BoolType.True);
                source.Capabilities.CapDuplexEnabled.SetValue(BoolType.False);
                break;
            case PaperSource.Duplex:
                source.Capabilities.CapFeederEnabled.SetValue(BoolType.True);
                source.Capabilities.CapDuplexEnabled.SetValue(BoolType.True);
                break;
        }

        // TODO: Should we add an "Automatic" option in the NAPS2 GUI instead of making "Glass" = Auto?
        // For "Auto", choose the feeder if it has paper, otherwise the flatbed.
        if (_options.PaperSource == PaperSource.Auto)
        {
            if (source.Capabilities.CapAutomaticSenseMedium.IsSupported)
            {
                source.Capabilities.CapAutomaticSenseMedium.SetValue(BoolType.True);
            }
            else if (source.Capabilities.CapFeederLoaded.IsSupported &&
                     source.Capabilities.CapFeederLoaded.GetCurrent() == BoolType.True)
            {
                source.Capabilities.CapFeederEnabled.SetValue(BoolType.True);
            }
        }

        // Bit Depth
        switch (_options.BitDepth)
        {
            case BitDepth.Color:
                source.Capabilities.ICapPixelType.SetValue(PixelType.RGB);
                source.Capabilities.ICapBitDepth.SetValue(24);
                break;
            case BitDepth.Grayscale:
                source.Capabilities.ICapPixelType.SetValue(PixelType.Gray);
                source.Capabilities.ICapBitDepth.SetValue(8);
                break;
            case BitDepth.BlackAndWhite:
                source.Capabilities.ICapPixelType.SetValue(PixelType.BlackWhite);
                source.Capabilities.ICapBitDepth.SetValue(1);
                break;
        }

        // Page Size, Horizontal Align
        float pageWidth = _options.PageSize!.WidthInThousandthsOfAnInch / 1000.0f;
        float pageHeight = _options.PageSize.HeightInThousandthsOfAnInch / 1000.0f;
        var pageMaxWidthFixed = source.Capabilities.ICapPhysicalWidth.GetCurrent();
        float pageMaxWidth = pageMaxWidthFixed.Whole + (pageMaxWidthFixed.Fraction / (float) UInt16.MaxValue);

        float horizontalOffset = 0.0f;
        if (_options.PageAlign == HorizontalAlign.Center)
            horizontalOffset = (pageMaxWidth - pageWidth) / 2;
        else if (_options.PageAlign == HorizontalAlign.Left)
            horizontalOffset = (pageMaxWidth - pageWidth);

        source.Capabilities.ICapUnits.SetValue(Unit.Inches);
        source.DGImage.ImageLayout.Get(out TWImageLayout imageLayout);
        imageLayout.Frame = new TWFrame
        {
            Left = horizontalOffset,
            Right = horizontalOffset + pageWidth,
            Top = 0,
            Bottom = pageHeight
        };
        source.DGImage.ImageLayout.Set(imageLayout);

        // Brightness, Contrast
        // Conveniently, the range of values used in settings (-1000 to +1000) is the same range TWAIN supports
        if (!_options.BrightnessContrastAfterScan)
        {
            source.Capabilities.ICapBrightness.SetValue(_options.Brightness);
            source.Capabilities.ICapContrast.SetValue(_options.Contrast);
        }

        // Resolution
        SetClosest(source.Capabilities.ICapXResolution, _options.Dpi);
        SetClosest(source.Capabilities.ICapYResolution, _options.Dpi);
    }

    private void SetClosest(ICapWrapper<TWFix32> cap, int value)
    {
        if (!cap.CanGet)
        {
            cap.SetValue(value);
            return;
        }
        var possibleValues = cap.GetValues().ToList();
        if (possibleValues.Count == 0)
        {
            cap.SetValue(value);
            return;
        }
        var closest = possibleValues.OrderBy(v => Math.Abs(v - value)).First();
        cap.SetValue(closest);
    }
}

#endif