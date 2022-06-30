using System.Threading;
using Google.Protobuf;
using NAPS2.Remoting.Worker;
using NAPS2.Scan.Exceptions;
using NTwain;
using NTwain.Data;

namespace NAPS2.Scan.Internal.Twain;

internal class TwainSessionRunner
{
    private readonly TwainDsm _dsm;
    private readonly ScanOptions _options;
    private readonly CancellationToken _cancelToken;
    private readonly ITwainEvents _twainEvents;
    private readonly TwainSession _session;
    private readonly TaskCompletionSource<bool> _tcs;
    private DataSource? _source;

    public TwainSessionRunner(TwainDsm dsm, ScanOptions options, CancellationToken cancelToken,
        ITwainEvents twainEvents)
    {
        _dsm = dsm;
        _options = options;
        _cancelToken = cancelToken;
        _twainEvents = twainEvents;

        PlatformInfo.Current.PreferNewDSM = dsm != TwainDsm.Old;
        _session = new TwainSession(TwainScanDriver.TwainAppId);
        _session.TransferReady += TransferReady;
        _session.DataTransferred += DataTransferred;
        _session.TransferError += TransferError;
        _session.SourceDisabled += SourceDisabled;
        _session.StateChanged += StateChanged;
        _tcs = new TaskCompletionSource<bool>();
    }

    public Task Run()
    {
        Invoker.Current.Invoke(Init);
        return _tcs.Task;
    }

    private void Init()
    {
        try
        {
            Debug.WriteLine("NAPS2.TW - Opening session");
            var rc = _options.DialogParent != IntPtr.Zero
                ? _session.Open(new WindowsFormsMessageLoopHook(_options.DialogParent))
                : _session.Open(); 
            if (rc != ReturnCode.Success)
            {
                throw new Exception($"TWAIN session open error: {rc}");
            }
            
            Debug.WriteLine("NAPS2.TW - Finding source");
            _source = _session.FirstOrDefault(x => x.Name == _options.Device.ID);
            if (_source == null)
            {
                throw new DeviceNotFoundException();
            }
            
            Debug.WriteLine("NAPS2.TW - Opening source");
            rc = _source.Open();
            if (rc != ReturnCode.Success)
            {
                throw new Exception($"TWAIN source open error: {rc}");
            }
            
            Debug.WriteLine("NAPS2.TW - Configuring source");
            ConfigureSource(_source);
            
            Debug.WriteLine("NAPS2.TW - Enabling source");
            var ui = _options.UseNativeUI ? SourceEnableMode.ShowUI : SourceEnableMode.NoUI;
            rc = _source.Enable(ui, true, _options.DialogParent);
            if (rc != ReturnCode.Success)
            {
                throw new Exception($"TWAIN source enable error: {rc}");
            }

            _cancelToken.Register(FinishWithCancellation);
        }
        catch (Exception ex)
        {
            FinishWithError(ex);
        }
    }

    private void FinishWithCancellation()
    {
        Debug.WriteLine("NAPS2.TW - Finishing with cancellation");
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
            Debug.WriteLine("NAPS2.TW - Will cancel via TransferReady");
        }
    }

    private void FinishWithError(Exception ex)
    {
        Debug.WriteLine("NAPS2.TW - Finishing with error");
        // If we're in state 5 or higher, we'll call ForceStepDown, which could potentially produce additional errors,
        // but what alternative is there?
        // If we're in state 4 or lower, this will just clean up the source/session.
        UnloadTwain();
        _tcs.TrySetException(ex);
    }

    private void FinishWithCompletion()
    {
        Debug.WriteLine("NAPS2.TW - Finishing with completion");
        // At this point we should be in state 4 and this will clean up the source/session.
        UnloadTwain();
        _tcs.TrySetResult(true);
    }

    private void UnloadTwain()
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

    private void StateChanged(object? sender, EventArgs e)
    {
        Debug.WriteLine($"NAPS2.TW - StateChanged (to {_session.State})");
    }

    private void SourceDisabled(object? sender, EventArgs e)
    {
        Debug.WriteLine("NAPS2.TW - SourceDisabled");
        FinishWithCompletion();
    }

    private void TransferError(object? sender, TransferErrorEventArgs e)
    {
        Debug.WriteLine("NAPS2.TW - TransferError");
        FinishWithError(e.Exception ?? new Exception($"TWAIN transfer error: {e.ReturnCode}"));
    }

    private void DataTransferred(object? sender, DataTransferredEventArgs e)
    {
        Debug.WriteLine("NAPS2.TW - DataTransferred");
        try
        {
            if (_options.TwainOptions.TransferMode == TwainTransferMode.Memory)
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
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error sending TWAIN data transfer event", ex);
        }
    }

    private void TransferReady(object? sender, TransferReadyEventArgs e)
    {
        Debug.WriteLine("NAPS2.TW - TransferReady");
        try
        {
            var pageStart = new TwainPageStart();
            if (_options.TwainOptions.TransferMode == TwainTransferMode.Memory)
            {
                pageStart.ImageData = ToImageData(e.PendingImageInfo);
            }
            _twainEvents.PageStart(pageStart);
            if (_cancelToken.IsCancellationRequested)
            {
                e.CancelAll = true;
            }
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error sending TWAIN transfer ready event", ex);
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
        if (_options.UseNativeUI)
        {
            return;
        }

        // Transfer Mode
        if (_options.TwainOptions.TransferMode == TwainTransferMode.Memory)
        {
            source.Capabilities.ICapXferMech.SetValue(XferMech.Memory);
        }

        // Progress UI
        if (!_options.TwainOptions.ShowProgress)
        {
            source.Capabilities.CapIndicators.SetValue(BoolType.False);
        }

        // Paper Source
        switch (_options.PaperSource)
        {
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

        // Bit Depth
        switch (_options.BitDepth)
        {
            case BitDepth.Color:
                source.Capabilities.ICapPixelType.SetValue(PixelType.RGB);
                break;
            case BitDepth.Grayscale:
                source.Capabilities.ICapPixelType.SetValue(PixelType.Gray);
                break;
            case BitDepth.BlackAndWhite:
                source.Capabilities.ICapPixelType.SetValue(PixelType.BlackWhite);
                break;
        }

        // Page Size, Horizontal Align
        float pageWidth = _options.PageSize.WidthInThousandthsOfAnInch / 1000.0f;
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
        source.Capabilities.ICapXResolution.SetValue(_options.Dpi);
        source.Capabilities.ICapYResolution.SetValue(_options.Dpi);
    }
}