using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
        private readonly ScannedImageHelper scannedImageHelper;

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

        public TwainWrapper(IFormFactory formFactory, IBlankDetector blankDetector, ThumbnailRenderer thumbnailRenderer, ScannedImageHelper scannedImageHelper)
        {
            this.formFactory = formFactory;
            this.blankDetector = blankDetector;
            this.thumbnailRenderer = thumbnailRenderer;
            this.scannedImageHelper = scannedImageHelper;
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
                try
                {
                    Debug.WriteLine("NAPS2.TW - DataTransferred");
                    pageNumber++;
                    using (var output = scanProfile.TwainImpl == TwainImpl.MemXfer
                                        ? GetBitmapFromMemXFer(eventArgs.MemoryData, eventArgs.ImageInfo)
                                        : Image.FromStream(eventArgs.GetNativeImageStream()))
                    {
                        using (var result = scannedImageHelper.PostProcessStep1(output, scanProfile))
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
                            scannedImageHelper.PostProcessStep2(image, result, scanProfile, scanParams, pageNumber);
                            images.Add(image);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("NAPS2.TW - DataTransferred - Error");
                    error = ex;
                    cancel = true;
                    twainForm.Close();
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

        private static Bitmap GetBitmapFromMemXFer(byte[] memoryData, TWImageInfo imageInfo)
        {
            int bytesPerPixel = memoryData.Length / (imageInfo.ImageWidth * imageInfo.ImageLength);
            PixelFormat pixelFormat = bytesPerPixel == 0 ? PixelFormat.Format1bppIndexed : PixelFormat.Format24bppRgb;
            int imageWidth = imageInfo.ImageWidth;
            int imageHeight = imageInfo.ImageLength;
            var bitmap = new Bitmap(imageWidth, imageHeight, pixelFormat);
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            try
            {
                byte[] source = memoryData;
                if (bytesPerPixel == 1)
                {
                    // No 8-bit greyscale format, so we have to transform into 24-bit
                    int rowWidth = data.Stride;
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
                else
                {
                    // Colors are provided as BGR, they need to be swapped to RGB
                    int rowWidth = data.Stride;
                    for (int row = 0; row < imageHeight; row++)
                    {
                        for (int col = 0; col < imageWidth; col++)
                        {
                            (source[row * rowWidth + col * 3], source[row * rowWidth + col * 3 + 2]) =
                                (source[row * rowWidth + col * 3 + 2], source[row * rowWidth + col * 3]);
                        }
                    }
                }
                Marshal.Copy(source, 0, data.Scan0, source.Length);
            }
            finally
            {
                bitmap.UnlockBits(data);
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

        private void ConfigureDS(DataSource ds, ScanProfile scanProfile, ScanParams scanParams)
        {
            if (scanProfile.UseNativeUI)
            {
                return;
            }

            // Transfer Mode
            if (scanProfile.TwainImpl == TwainImpl.MemXfer)
            {
                ds.Capabilities.ICapXferMech.SetValue(XferMech.Memory);
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
