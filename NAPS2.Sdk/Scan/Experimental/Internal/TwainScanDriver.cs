using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Images.Storage;
using NAPS2.Logging;
using NAPS2.Platform;
using NAPS2.Scan.Exceptions;
using NAPS2.Util;
using NAPS2.WinForms;
using NTwain;
using NTwain.Data;

namespace NAPS2.Scan.Experimental.Internal
{
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

        private readonly ImageContext imageContext;

        public TwainScanDriver(ImageContext imageContext)
        {
            this.imageContext = imageContext;
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

        public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IImage> callback)
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

        private void InternalScan(TwainDsm dsm, ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IImage> callback)
        {
            var dialogParent = options.DialogParent;
            if (dialogParent == IntPtr.Zero)
            {
                dialogParent = new BackgroundForm().Handle;
            }

            PlatformInfo.Current.PreferNewDSM = dsm != TwainDsm.Old;
            var session = new TwainSession(TwainAppId);
            var twainForm = Invoker.Current.InvokeGet(() => options.NoUI ? null : new FTwainGui());
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
                    using (var image = options.TwainOptions.TransferMode == TwainTransferMode.Memory
                                        ? GetBitmapFromMemXFer(eventArgs.MemoryData, eventArgs.ImageInfo)
                                        : imageContext.ImageFactory.Decode(eventArgs.GetNativeImageStream(), ".bmp"))
                    {
                        // TODO: Pipe back patch codes somehow... Or maybe just rely solely on Zxing
                        //if (scanParams.DetectPatchCodes)
                        //{
                        //    foreach (var patchCodeInfo in eventArgs.GetExtImageInfo(ExtendedImageInfo.PatchCode))
                        //    {
                        //        if (patchCodeInfo.ReturnCode == ReturnCode.Success)
                        //        {
                        //            image.PatchCode = GetPatchCode(patchCodeInfo);
                        //        }
                        //    }
                        //}
                        callback(image);
                    }
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
                if (!options.NoUI)
                {
                    Invoker.Current.Invoke(() => twainForm.Close());
                }
            }

            void InitTwain()
            {
                try
                {
                    var windowHandle = (Invoker.Current as Form)?.Handle;
                    ReturnCode rc = windowHandle != null ? session.Open(new WindowsFormsMessageLoopHook(windowHandle.Value)) : session.Open();
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
                    rc = options.NoUI ? ds.Enable(ui, true, windowHandle ?? IntPtr.Zero) : ds.Enable(ui, true, twainForm.Handle);
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

            if (!options.NoUI)
            {
                twainForm.Shown += (sender, eventArgs) => { InitTwain(); };
                twainForm.Closed += (sender, args) => waitHandle.Set();
            }

            if (options.NoUI)
            {
                Debug.WriteLine("NAPS2.TW - Init with no form");
                Invoker.Current.Invoke(InitTwain);
            }
            else if (!options.Modal)
            {
                Debug.WriteLine("NAPS2.TW - Init with non-modal form");
                Invoker.Current.Invoke(() => twainForm.Show(new Win32Window(dialogParent)));
            }
            else
            {
                Debug.WriteLine("NAPS2.TW - Init with modal form");
                Invoker.Current.Invoke(() => twainForm.ShowDialog(new Win32Window(dialogParent)));
            }
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

        private IImage GetBitmapFromMemXFer(byte[] memoryData, TWImageInfo imageInfo)
        {
            int bytesPerPixel = memoryData.Length / (imageInfo.ImageWidth * imageInfo.ImageLength);
            var pixelFormat = bytesPerPixel == 0 ? StoragePixelFormat.BW1 : StoragePixelFormat.RGB24;
            int imageWidth = imageInfo.ImageWidth;
            int imageHeight = imageInfo.ImageLength;
            var bitmap = imageContext.ImageFactory.FromDimensions(imageWidth, imageHeight, pixelFormat);
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

        private static PatchCode GetPatchCode(TWInfo patchCodeInfo)
        {
            switch ((NTwain.Data.PatchCode)patchCodeInfo.Item)
            {
                case NTwain.Data.PatchCode.Patch1:
                    return PatchCode.Patch1;
                case NTwain.Data.PatchCode.Patch2:
                    return PatchCode.Patch2;
                case NTwain.Data.PatchCode.Patch3:
                    return PatchCode.Patch3;
                case NTwain.Data.PatchCode.Patch4:
                    return PatchCode.Patch4;
                case NTwain.Data.PatchCode.Patch6:
                    return PatchCode.Patch6;
                case NTwain.Data.PatchCode.PatchT:
                    return PatchCode.PatchT;
                default:
                    throw new ArgumentException();
            }
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

            // Hide UI for console
            if (options.NoUI)
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

            // Patch codes
            if (options.DetectPatchCodes)
            {
                ds.Capabilities.ICapPatchCodeDetectionEnabled.SetValue(BoolType.True);
            }
        }
    }
}