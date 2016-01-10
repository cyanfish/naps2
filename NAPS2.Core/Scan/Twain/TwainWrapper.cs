using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.WinForms;
using NTwain;
using NTwain.Data;

namespace NAPS2.Scan.Twain
{
    public class TwainWrapper
    {
        private static readonly TWIdentity TwainAppId = TWIdentity.CreateFromAssembly(DataGroups.Image | DataGroups.Control, Assembly.GetEntryAssembly());

        private readonly IFormFactory formFactory;

        static TwainWrapper()
        {
            PlatformInfo.Current.PreferNewDSM = false;
#if DEBUG
            PlatformInfo.Current.Log.IsDebugEnabled = true;
#endif
        }

        public TwainWrapper(IFormFactory formFactory)
        {
            this.formFactory = formFactory;
        }

        public ScanDevice PromptForDevice()
        {
            //if (ScanProfile != null && ScanProfile.TwainImpl == TwainImpl.Legacy)
            //{
            //    return Legacy.TwainApi.SelectDeviceUI();
            //}

            var session = new TwainSession(TwainAppId);
            session.Open();
            try
            {
                var ds = session.ShowSourceSelector();
                if (ds == null)
                {
                    return null;
                }
                string deviceId = ds.Name;
                string deviceName = ds.Name;
                return new ScanDevice(deviceId, deviceName);
            }
            finally
            {
                session.Close();
            }
        }

        public List<ScannedImage> Scan(Form dialogParent, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams)
        {
            //if (ScanProfile.TwainImpl == TwainImpl.Legacy)
            //{
            //    return Legacy.TwainApi.Scan(ScanProfile, ScanDevice, DialogParent, formFactory, scannedImageFactory);
            //}

            var session = new TwainSession(TwainAppId);
            var twainForm = formFactory.Create<FTwainGui>();
            var images = new List<ScannedImage>();
            Exception error = null;
            bool cancel = false;
            DataSource ds = null;

            session.TransferReady += (sender, eventArgs) =>
            {
                Debug.WriteLine("NAPS2.TW - TransferReady");
                if (cancel)
                {
                    eventArgs.CancelAll = true;
                }
            };
            session.DataTransferred += (sender, eventArgs) =>
            {
                Debug.WriteLine("NAPS2.TW - DataTransferred");
                using (var output = Image.FromStream(eventArgs.GetNativeImageStream()))
                {
                    double scaleFactor = 1;
                    if (!scanProfile.UseNativeUI)
                    {
                        scaleFactor = scanProfile.AfterScanScale.ToIntScaleFactor();
                    }

                    using (var result = ImageScaleHelper.ScaleImage(output, scaleFactor))
                    {
                        var bitDepth = output.PixelFormat == PixelFormat.Format1bppIndexed
                            ? ScanBitDepth.BlackWhite
                            : ScanBitDepth.C24Bit;
                        var img = new ScannedImage(result, bitDepth, scanProfile.MaxQuality, scanProfile.Quality);
                        if (scanParams.DetectPatchCodes)
                        {
                            foreach (var patchCodeInfo in eventArgs.GetExtImageInfo(ExtendedImageInfo.PatchCode))
                            {
                                if (patchCodeInfo.ReturnCode == ReturnCode.Success)
                                {
                                    img.PatchCode = GetPatchCode(patchCodeInfo);
                                }
                            }
                        }
                        images.Add(img);
                    }
                }
            };
            session.TransferError += (sender, eventArgs) =>
            {
                Debug.WriteLine("NAPS2.TW - TransferError - {0}", eventArgs.Exception);
                error = eventArgs.Exception;
                cancel = true;
                twainForm.Close();
            };
            session.SourceDisabled += (sender, eventArgs) =>
            {
                Debug.WriteLine("NAPS2.TW - SourceDisabled");
                twainForm.Close();
            };

            twainForm.Shown += (sender, eventArgs) =>
            {
                Debug.WriteLine("NAPS2.TW - TwainForm.Shown");
                try
                {
                    ReturnCode rc = session.Open(new WindowsFormsMessageLoopHook(dialogParent.Handle));
                    if (rc != ReturnCode.Success)
                    {
                        Debug.WriteLine("NAPS2.TW - Could not open session - {0}", rc);
                        twainForm.Close();
                        return;
                    }
                    ds = session.FirstOrDefault(x => x.Name == scanDevice.ID);
                    if (ds == null)
                    {
                        Debug.WriteLine("NAPS2.TW - Could not find DS - DS count = {0}", session.Count());
                        throw new DeviceNotFoundException();
                    }
                    rc = ds.Open();
                    if (rc != ReturnCode.Success)
                    {
                        Debug.WriteLine("NAPS2.TW - Could not open DS - {0}", rc);
                        twainForm.Close();
                        return;
                    }
                    ConfigureDS(ds, scanProfile, scanParams);
                    var ui = scanProfile.UseNativeUI ? SourceEnableMode.ShowUI : SourceEnableMode.NoUI;
                    Debug.WriteLine("NAPS2.TW - Enabling DS");
                    rc = ds.Enable(ui, true, twainForm.Handle);
                    Debug.WriteLine("NAPS2.TW - Enable finished");
                    if (rc != ReturnCode.Success)
                    {
                        Debug.WriteLine("NAPS2.TW - Enable failed - {0}, rc");
                        twainForm.Close();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("NAPS2.TW - Error");
                    error = ex;
                    twainForm.Close();
                }
            };

            Debug.WriteLine("NAPS2.TW - Showing TwainForm");
            twainForm.ShowDialog(dialogParent);
            Debug.WriteLine("NAPS2.TW - TwainForm closed");

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

            return images;
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

        private void ConfigureDS(DataSource ds, ScanProfile scanProfile, ScanParams scanParams)
        {
            if (scanProfile.UseNativeUI)
            {
                return;
            }

            // Paper Source
            switch (scanProfile.PaperSource)
            {
                case ScanSource.Glass:
                    ds.Capabilities.CapFeederEnabled.SetValue(BoolType.False);
                    ds.Capabilities.CapDuplexEnabled.SetValue(BoolType.False);
                    break;
                case ScanSource.Feeder:
                    ds.Capabilities.CapFeederEnabled.SetValue(BoolType.True);
                    ds.Capabilities.CapDuplexEnabled.SetValue(BoolType.False);
                    break;
                case ScanSource.Duplex:
                    ds.Capabilities.CapFeederEnabled.SetValue(BoolType.True);
                    ds.Capabilities.CapDuplexEnabled.SetValue(BoolType.True);
                    break;
            }

            // Bit Depth
            switch (scanProfile.BitDepth)
            {
                case ScanBitDepth.C24Bit:
                    ds.Capabilities.ICapPixelType.SetValue(PixelType.RGB);
                    break;
                case ScanBitDepth.Grayscale:
                    ds.Capabilities.ICapPixelType.SetValue(PixelType.Gray);
                    break;
                case ScanBitDepth.BlackWhite:
                    ds.Capabilities.ICapPixelType.SetValue(PixelType.BlackWhite);
                    break;
            }

            // Page Size, Horizontal Align
            PageDimensions pageDimensions = scanProfile.PageSize.PageDimensions() ?? scanProfile.CustomPageSize;
            if (pageDimensions == null)
            {
                throw new InvalidOperationException("No page size specified");
            }
            float pageWidth = pageDimensions.WidthInThousandthsOfAnInch() / 1000.0f;
            float pageHeight = pageDimensions.HeightInThousandthsOfAnInch() / 1000.0f;
            var pageMaxWidthFixed = ds.Capabilities.ICapPhysicalWidth.GetCurrent();
            float pageMaxWidth = pageMaxWidthFixed.Whole + (pageMaxWidthFixed.Fraction / (float)UInt16.MaxValue);

            float horizontalOffset = 0.0f;
            if (scanProfile.PageAlign == ScanHorizontalAlign.Center)
                horizontalOffset = (pageMaxWidth - pageWidth) / 2;
            else if (scanProfile.PageAlign == ScanHorizontalAlign.Left)
                horizontalOffset = (pageMaxWidth - pageWidth);

            ds.Capabilities.ICapUnits.SetValue(Unit.Inches);
            TWImageLayout imageLayout;
            ds.DGImage.ImageLayout.Get(out imageLayout);
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
            if (!scanProfile.BrightnessContrastAfterScan)
            {
                ds.Capabilities.ICapBrightness.SetValue(scanProfile.Brightness);
                ds.Capabilities.ICapContrast.SetValue(scanProfile.Contrast);
            }

            // Resolution
            int dpi = scanProfile.Resolution.ToIntDpi();
            ds.Capabilities.ICapXResolution.SetValue(dpi);
            ds.Capabilities.ICapYResolution.SetValue(dpi);

            // Patch codes
            if (scanParams.DetectPatchCodes)
            {
                ds.Capabilities.ICapPatchCodeDetectionEnabled.SetValue(BoolType.True);
            }
        }
    }
}
