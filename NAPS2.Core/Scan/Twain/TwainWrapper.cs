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
using NAPS2.Util;

namespace NAPS2.Scan.Twain
{
    public class TwainWrapper
    {
        private static readonly TWIdentity TwainAppId = TWIdentity.CreateFromAssembly(DataGroups.Image | DataGroups.Control, Assembly.GetEntryAssembly());

        private readonly IFormFactory formFactory;
        private readonly IBlankDetector blankDetector;
        private readonly ThumbnailRenderer thumbnailRenderer;

        static TwainWrapper()
        {
#if STANDALONE
            // Path to the folder containing the 64-bit twaindsm.dll relative to NAPS2.Core.dll
            const string lib64Dir = "64";
            if (Environment.Is64BitProcess)
            {
                var location = Assembly.GetExecutingAssembly().Location;
                var coreDllDir = System.IO.Path.GetDirectoryName(location);
                if (coreDllDir != null)
                {
                    Win32.SetDllDirectory(System.IO.Path.Combine(coreDllDir, lib64Dir));
                }
            }
#endif
#if DEBUG
            PlatformInfo.Current.Log.IsDebugEnabled = true;
#endif
        }

        public TwainWrapper(IFormFactory formFactory, IBlankDetector blankDetector, ThumbnailRenderer thumbnailRenderer)
        {
            this.formFactory = formFactory;
            this.blankDetector = blankDetector;
            this.thumbnailRenderer = thumbnailRenderer;
        }

        public List<ScanDevice> GetDeviceList(TwainImpl twainImpl)
        {
            PlatformInfo.Current.PreferNewDSM = twainImpl != TwainImpl.OldDsm;
            var session = new TwainSession(TwainAppId);
            session.Open();
            try
            {
                return session.GetSources().Select(ds => new ScanDevice(ds.Name, ds.Name)).ToList();
            }
            finally
            {
                session.Close();
            }
        }

        public List<ScannedImage> Scan(IWin32Window dialogParent, bool activate, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams)
        {
            if (scanProfile.TwainImpl == TwainImpl.Legacy)
            {
                return Legacy.TwainApi.Scan(scanProfile, scanDevice, dialogParent, formFactory);
            }

            PlatformInfo.Current.PreferNewDSM = scanProfile.TwainImpl != TwainImpl.OldDsm;
            var session = new TwainSession(TwainAppId);
            var twainForm = formFactory.Create<FTwainGui>();
            var images = new List<ScannedImage>();
            Exception error = null;
            bool cancel = false;
            DataSource ds = null;

            int pageNumber = 0;

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
                pageNumber++;
                using (var output = Image.FromStream(eventArgs.GetNativeImageStream()))
                {
                    using (var result = ScannedImageHelper.PostProcessStep1(output, scanProfile))
                    {
                        if (blankDetector.ExcludePage(result, scanProfile))
                        {
                            return;
                        }

                        var bitDepth = output.PixelFormat == PixelFormat.Format1bppIndexed
                            ? ScanBitDepth.BlackWhite
                            : ScanBitDepth.C24Bit;
                        var image = new ScannedImage(result, bitDepth, scanProfile.MaxQuality, scanProfile.Quality);
                        image.SetThumbnail(thumbnailRenderer.RenderThumbnail(result));
                        if (scanParams.DetectPatchCodes)
                        {
                            foreach (var patchCodeInfo in eventArgs.GetExtImageInfo(ExtendedImageInfo.PatchCode))
                            {
                                if (patchCodeInfo.ReturnCode == ReturnCode.Success)
                                {
                                    image.PatchCode = GetPatchCode(patchCodeInfo);
                                }
                            }
                        }
                        ScannedImageHelper.PostProcessStep2(image, result, scanProfile, scanParams, pageNumber);
                        images.Add(image);
                    }
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
                twainForm.Close();
            };
            session.SourceDisabled += (sender, eventArgs) =>
            {
                Debug.WriteLine("NAPS2.TW - SourceDisabled");
                twainForm.Close();
            };

            twainForm.Shown += (sender, eventArgs) =>
            {
                if (activate)
                {
                    // TODO: Set this flag based on whether NAPS2 already has focus
                    // http://stackoverflow.com/questions/7162834/determine-if-current-application-is-activated-has-focus
                    // Or maybe http://stackoverflow.com/questions/156046/show-a-form-without-stealing-focus
                    twainForm.Activate();
                }
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
