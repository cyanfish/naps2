using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using NAPS2.Platform.Windows;
using NAPS2.Scan.Exceptions;
using NTwain;
using NTwain.Data;

namespace NAPS2.Scan.Internal;

internal class TwainScanDriver : IScanDriver
{
    private static readonly TWIdentity TwainAppId = TWIdentity.CreateFromAssembly(DataGroups.Image | DataGroups.Control, Assembly.GetEntryAssembly());

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

    public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IMemoryImage> callback)
    {
        return Task.Run(() =>
        {
            try
            {
                InternalScan(options.TwainOptions.Dsm, options, cancelToken, scanEvents, callback);
            }
            catch (DeviceNotFoundException)
            {
                if (options.TwainOptions.Dsm != TwainDsm.Old)
                {
                    // Fall back to OldDsm in case of no devices
                    // This is primarily for Citrix support, which requires using twain_32.dll for TWAIN passthrough
                    InternalScan(TwainDsm.Old, options, cancelToken, scanEvents, callback);
                }
                else
                {
                    throw;
                }
            }
        });
    }

    private void InternalScan(TwainDsm dsm, ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IMemoryImage> callback)
    {
        PlatformInfo.Current.PreferNewDSM = dsm != TwainDsm.Old;
        var session = new TwainSession(TwainAppId);
        Exception error = null;
        bool cancel = false;
        DataSource ds = null;
        var waitHandle = new AutoResetEvent(false);

        session.TransferReady += (sender, eventArgs) =>
        {
            Debug.WriteLine("NAPS2.TW - TransferReady");
            scanEvents.PageStart();
            if (cancel)
            {
                eventArgs.CancelAll = true;
            }
        };
        session.DataTransferred += (sender, eventArgs) =>
        {
            try
            {
                Debug.WriteLine("NAPS2.TW - DataTransferred");
                using var image = options.TwainOptions.TransferMode == TwainTransferMode.Memory
                    ? GetBitmapFromMemXFer(eventArgs.MemoryData, eventArgs.ImageInfo)
                    : _scanningContext.ImageContext.Load(eventArgs.GetNativeImageStream());
                callback(image);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("NAPS2.TW - DataTransferred - Error");
                error = ex;
                cancel = true;
                StopTwain();
            }
        };
        session.TransferError += (sender, eventArgs) =>
        {
            Debug.WriteLine("NAPS2.TW - TransferError");
            if (eventArgs.Exception != null)
            {
                error = eventArgs.Exception;
            }
            else if (eventArgs.SourceStatus != null)
            {
                Log.Error("TWAIN Transfer Error. Return code = {0}; condition code = {1}; data = {2}.",
                    eventArgs.ReturnCode, eventArgs.SourceStatus.ConditionCode, eventArgs.SourceStatus.Data);
            }
            else
            {
                Log.Error("TWAIN Transfer Error. Return code = {0}.", eventArgs.ReturnCode);
            }
            cancel = true;
            StopTwain();
        };
        session.SourceDisabled += (sender, eventArgs) =>
        {
            Debug.WriteLine("NAPS2.TW - SourceDisabled");
            StopTwain();
        };

        void StopTwain()
        {
            waitHandle.Set();
        }

        void InitTwain()
        {
            try
            {
                var windowHandle = options.DialogParent;
                ReturnCode rc = windowHandle != IntPtr.Zero ? session.Open(new WindowsFormsMessageLoopHook(windowHandle)) : session.Open();
                if (rc != ReturnCode.Success)
                {
                    Debug.WriteLine("NAPS2.TW - Could not open session - {0}", rc);
                    StopTwain();
                    return;
                }
                ds = session.FirstOrDefault(x => x.Name == options.Device.ID);
                if (ds == null)
                {
                    Debug.WriteLine("NAPS2.TW - Could not find DS - DS count = {0}", session.Count());
                    throw new DeviceNotFoundException();
                }
                rc = ds.Open();
                if (rc != ReturnCode.Success)
                {
                    Debug.WriteLine("NAPS2.TW - Could not open DS - {0}", rc);
                    StopTwain();
                    return;
                }
                ConfigureDS(ds, options);
                var ui = options.UseNativeUI ? SourceEnableMode.ShowUI : SourceEnableMode.NoUI;
                Debug.WriteLine("NAPS2.TW - Enabling DS");
                rc = ds.Enable(ui, true, windowHandle);
                Debug.WriteLine("NAPS2.TW - Enable finished");
                if (rc != ReturnCode.Success)
                {
                    Debug.WriteLine("NAPS2.TW - Enable failed - {0}, rc");
                    StopTwain();
                }
                else
                {
                    cancelToken.Register(() =>
                    {
                        Debug.WriteLine("NAPS2.TW - User Cancel");
                        cancel = true;
                        session.ForceStepDown(5);
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("NAPS2.TW - Error");
                error = ex;
                StopTwain();
            }
        }

        Debug.WriteLine("NAPS2.TW - Init");
        Invoker.Current.Invoke(InitTwain);
        waitHandle.WaitOne();
        Debug.WriteLine("NAPS2.TW - Operation complete");

        if (ds != null && session.IsSourceOpen)
        {
            Debug.WriteLine("NAPS2.TW - Closing DS");
            ds.Close();
        }
        if (session.IsDsmOpen)
        {
            Debug.WriteLine("NAPS2.TW - Closing session");
            session.Close();
        }

        if (error != null)
        {
            Debug.WriteLine("NAPS2.TW - Throwing error - {0}", error);
            if (error is ScanDriverException)
            {
                throw error;
            }
            throw new ScanDriverUnknownException(error);
        }
    }

    private IMemoryImage GetBitmapFromMemXFer(byte[] memoryData, TWImageInfo imageInfo)
    {
        int bytesPerPixel = memoryData.Length / (imageInfo.ImageWidth * imageInfo.ImageLength);
        var pixelFormat = bytesPerPixel == 0 ? ImagePixelFormat.BW1 : ImagePixelFormat.RGB24;
        int imageWidth = imageInfo.ImageWidth;
        int imageHeight = imageInfo.ImageLength;
        var bitmap = _scanningContext.ImageContext.Create(imageWidth, imageHeight, pixelFormat);
        var data = bitmap.Lock(LockMode.WriteOnly, out var scan0, out var stride);
        try
        {
            byte[] source = memoryData;
            if (bytesPerPixel == 1)
            {
                // No 8-bit greyscale format, so we have to transform into 24-bit
                int rowWidth = stride;
                int originalRowWidth = source.Length / imageHeight;
                byte[] source2 = new byte[rowWidth * imageHeight];
                for (int row = 0; row < imageHeight; row++)
                {
                    for (int col = 0; col < imageWidth; col++)
                    {
                        source2[row * rowWidth + col * 3] = source[row * originalRowWidth + col];
                        source2[row * rowWidth + col * 3 + 1] = source[row * originalRowWidth + col];
                        source2[row * rowWidth + col * 3 + 2] = source[row * originalRowWidth + col];
                    }
                }
                source = source2;
            }
            else if (bytesPerPixel == 3)
            {
                // Colors are provided as BGR, they need to be swapped to RGB
                int rowWidth = stride;
                for (int row = 0; row < imageHeight; row++)
                {
                    for (int col = 0; col < imageWidth; col++)
                    {
                        (source[row * rowWidth + col * 3], source[row * rowWidth + col * 3 + 2]) =
                            (source[row * rowWidth + col * 3 + 2], source[row * rowWidth + col * 3]);
                    }
                }
            }
            Marshal.Copy(source, 0, scan0, source.Length);
        }
        finally
        {
            bitmap.Unlock(data);
        }
        return bitmap;
    }

    private void ConfigureDS(DataSource ds, ScanOptions options)
    {
        if (options.UseNativeUI)
        {
            return;
        }

        // Transfer Mode
        if (options.TwainOptions.TransferMode == TwainTransferMode.Memory)
        {
            ds.Capabilities.ICapXferMech.SetValue(XferMech.Memory);
        }

        // Progress UI
        if (!options.TwainOptions.ShowProgress)
        {
            ds.Capabilities.CapIndicators.SetValue(BoolType.False);
        }

        // Paper Source
        switch (options.PaperSource)
        {
            case PaperSource.Flatbed:
                ds.Capabilities.CapFeederEnabled.SetValue(BoolType.False);
                ds.Capabilities.CapDuplexEnabled.SetValue(BoolType.False);
                break;
            case PaperSource.Feeder:
                ds.Capabilities.CapFeederEnabled.SetValue(BoolType.True);
                ds.Capabilities.CapDuplexEnabled.SetValue(BoolType.False);
                break;
            case PaperSource.Duplex:
                ds.Capabilities.CapFeederEnabled.SetValue(BoolType.True);
                ds.Capabilities.CapDuplexEnabled.SetValue(BoolType.True);
                break;
        }

        // Bit Depth
        switch (options.BitDepth)
        {
            case BitDepth.Color:
                ds.Capabilities.ICapPixelType.SetValue(PixelType.RGB);
                break;
            case BitDepth.Grayscale:
                ds.Capabilities.ICapPixelType.SetValue(PixelType.Gray);
                break;
            case BitDepth.BlackAndWhite:
                ds.Capabilities.ICapPixelType.SetValue(PixelType.BlackWhite);
                break;
        }

        // Page Size, Horizontal Align
        float pageWidth = options.PageSize.WidthInThousandthsOfAnInch / 1000.0f;
        float pageHeight = options.PageSize.HeightInThousandthsOfAnInch / 1000.0f;
        var pageMaxWidthFixed = ds.Capabilities.ICapPhysicalWidth.GetCurrent();
        float pageMaxWidth = pageMaxWidthFixed.Whole + (pageMaxWidthFixed.Fraction / (float)UInt16.MaxValue);

        float horizontalOffset = 0.0f;
        if (options.PageAlign == HorizontalAlign.Center)
            horizontalOffset = (pageMaxWidth - pageWidth) / 2;
        else if (options.PageAlign == HorizontalAlign.Left)
            horizontalOffset = (pageMaxWidth - pageWidth);

        ds.Capabilities.ICapUnits.SetValue(Unit.Inches);
        ds.DGImage.ImageLayout.Get(out TWImageLayout imageLayout);
        imageLayout.Frame = new TWFrame
        {
            Left = horizontalOffset,
            Right = horizontalOffset + pageWidth,
            Top = 0,
            Bottom = pageHeight
        };
        ds.DGImage.ImageLayout.Set(imageLayout);

        // Brightness, Contrast
        // Conveniently, the range of values used in settings (-1000 to +1000) is the same range TWAIN supports
        if (!options.BrightnessContrastAfterScan)
        {
            ds.Capabilities.ICapBrightness.SetValue(options.Brightness);
            ds.Capabilities.ICapContrast.SetValue(options.Contrast);
        }

        // Resolution
        ds.Capabilities.ICapXResolution.SetValue(options.Dpi);
        ds.Capabilities.ICapYResolution.SetValue(options.Dpi);
    }
}