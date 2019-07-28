using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Operation;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.Scan.Wia.Native;
using NAPS2.Util;
using NAPS2.Worker;

namespace NAPS2.Scan.Wia
{
    public class WiaScanOperation : OperationBase
    {
        private readonly ScannedImageHelper scannedImageHelper;
        private readonly IBlankDetector blankDetector;
        private readonly IWorkerServiceFactory workerServiceFactory;

        private readonly SmoothProgress smoothProgress = new SmoothProgress();

        public WiaScanOperation(ScannedImageHelper scannedImageHelper, IBlankDetector blankDetector, IWorkerServiceFactory workerServiceFactory)
        {
            this.scannedImageHelper = scannedImageHelper;
            this.blankDetector = blankDetector;
            this.workerServiceFactory = workerServiceFactory;
            AllowCancel = true;
            AllowBackground = true;

            smoothProgress.OutputProgressChanged += SmoothProgressChanged;
        }

        private void SmoothProgressChanged(object sender, SmoothProgress.ProgressChangeEventArgs args)
        {
            Status.CurrentProgress = (int)(args.Value * Status.MaxProgress);
            InvokeStatusChanged();
        }

        public Exception ScanException { get; private set; }

        private ScanProfile ScanProfile { get; set; }

        private ScanDevice ScanDevice { get; set; }

        private ScanParams ScanParams { get; set; }

        private IWin32Window DialogParent { get; set; }

        public bool Start(ScanProfile scanProfile, ScanDevice scanDevice, ScanParams scanParams, IWin32Window dialogParent, ScannedImageSource.Concrete source)
        {
            ScanProfile = scanProfile;
            ScanDevice = scanDevice;
            ScanParams = scanParams;
            DialogParent = dialogParent;
            ProgressTitle = ScanDevice.Name;
            Status = new OperationStatus
            {
                StatusText = ScanProfile.PaperSource == ScanSource.Glass
                    ? MiscResources.AcquiringData
                    : string.Format(MiscResources.ScanProgressPage, 1),
                MaxProgress = 1000,
                ProgressType = OperationProgressType.BarOnly
            };

            // TODO: NoUI
            // TODO: Test native UI in console behaviour (versus older behaviour)
            // TODO: What happens if you close FDesktop while a batch scan is in progress?

            RunAsync(() =>
            {
                try
                {
                    try
                    {
                        smoothProgress.Reset();
                        Scan(source);
                    }
                    catch (WiaException e)
                    {
                        WiaScanErrors.ThrowDeviceError(e);
                    }

                    return true;
                }
                catch (Exception e)
                {
                    // Don't call InvokeError; the driver will do the actual error handling
                    ScanException = e;
                    return false;
                }
                finally
                {
                    smoothProgress.Reset();
                }
            });

            return true;
        }

        private void Scan(ScannedImageSource.Concrete source)
        {
            using (var deviceManager = new WiaDeviceManager(ScanProfile.WiaVersion))
            using (var device = deviceManager.FindDevice(ScanDevice.ID))
            {
                if (device.Version == WiaVersion.Wia20 && ScanProfile.UseNativeUI)
                {
                    DoWia20NativeTransfer(source, deviceManager, device);
                    return;
                }

                using (var item = GetItem(device))
                {
                    if (item == null)
                    {
                        return;
                    }

                    DoTransfer(source, device, item);
                }
            }
        }

        private void InitProgress(WiaDevice device)
        {
            ProgressTitle = device.Name();
            InvokeStatusChanged();
        }

        private void InitNextPageProgress(int pageNumber)
        {
            if (ScanProfile.PaperSource != ScanSource.Glass)
            {
                Status.StatusText = string.Format(MiscResources.ScanProgressPage, pageNumber);
                smoothProgress.Reset();
            }
        }

        private void ProduceImage(ScannedImageSource.Concrete source, Image output, ref int pageNumber)
        {
            using (var result = scannedImageHelper.PostProcessStep1(output, ScanProfile))
            {
                if (blankDetector.ExcludePage(result, ScanProfile))
                {
                    return;
                }

                ScanBitDepth bitDepth = ScanProfile.UseNativeUI ? ScanBitDepth.C24Bit : ScanProfile.BitDepth;
                var image = new ScannedImage(result, bitDepth, ScanProfile.MaxQuality, ScanProfile.Quality);
                scannedImageHelper.PostProcessStep2(image, result, ScanProfile, ScanParams, pageNumber);
                string tempPath = scannedImageHelper.SaveForBackgroundOcr(result, ScanParams);
                scannedImageHelper.RunBackgroundOcr(image, ScanParams, tempPath);
                source.Put(image);

                pageNumber++;
                InitNextPageProgress(pageNumber);
            }
        }

        private void DoWia20NativeTransfer(ScannedImageSource.Concrete source, WiaDeviceManager deviceManager, WiaDevice device)
        {
            // WIA 2.0 doesn't support normal transfers with native UI.
            // Instead we need to have it write the scans to a set of files and load those.

            var hwnd = Invoker.Current.InvokeGet(() => DialogParent.Handle);
            var paths = deviceManager.PromptForImage(hwnd, device);

            if (paths == null)
            {
                return;
            }

            int pageNumber = 1;
            InitProgress(device);

            try
            {
                foreach (var path in paths)
                {
                    using (var stream = new FileStream(path, FileMode.Open))
                    using (var output = Image.FromStream(stream))
                    {
                        int frameCount = output.GetFrameCount(FrameDimension.Page);
                        for (int i = 0; i < frameCount; i++)
                        {
                            output.SelectActiveFrame(FrameDimension.Page, i);
                            ProduceImage(source, output, ref pageNumber);
                        }
                    }
                }
            }
            finally
            {
                foreach (var path in paths)
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception e)
                    {
                        Log.ErrorException("Error deleting WIA 2.0 native transferred file", e);
                    }
                }
            }
        }

        private void DoTransfer(ScannedImageSource.Concrete source, WiaDevice device, WiaItem item)
        {
            if (ScanProfile.PaperSource != ScanSource.Glass && !device.SupportsFeeder())
            {
                throw new NoFeederSupportException();
            }
            if (ScanProfile.PaperSource == ScanSource.Duplex && !device.SupportsDuplex())
            {
                throw new NoDuplexSupportException();
            }

            InitProgress(device);
            ConfigureProps(device, item);

            using (var transfer = item.StartTransfer())
            {
                int pageNumber = 1;
                transfer.PageScanned += (sender, args) =>
                {
                    try
                    {
                        using (args.Stream)
                        {
                            if (args.Stream.Length == 0)
                            {
                                return;
                            }
                            using (var output = Image.FromStream(args.Stream))
                            {
                                ProduceImage(source, output, ref pageNumber);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ScanException = e;
                    }
                };
                transfer.Progress += (sender, args) => smoothProgress.InputProgressChanged(args.Percent / 100.0);
                using (CancelToken.Register(transfer.Cancel))
                {
                    transfer.Download();

                    if (device.Version == WiaVersion.Wia10 && ScanProfile.PaperSource != ScanSource.Glass)
                    {
                        // For WIA 1.0 feeder scans, we need to repeatedly call Download until WIA_ERROR_PAPER_EMPTY is received.
                        try
                        {
                            while (!CancelToken.IsCancellationRequested)
                            {
                                transfer.Download();
                            }
                        }
                        catch (WiaException e) when (e.ErrorCode == WiaErrorCodes.PAPER_EMPTY)
                        {
                        }
                    }
                }
            }
        }

        private WiaItem GetItem(WiaDevice device)
        {
            if (ScanProfile.UseNativeUI)
            {
                var hwnd = Invoker.Current.InvokeGet(() => DialogParent.Handle);
                bool useWorker = Environment.Is64BitProcess && device.Version == WiaVersion.Wia10;
                if (useWorker)
                {
                    WiaConfiguration config;
                    using (var worker = workerServiceFactory.Create())
                    {
                        config = worker.Service.Wia10NativeUI(device.Id(), hwnd);
                    }
                    var item = device.FindSubItem(config.ItemName);
                    device.Properties.DeserializeEditable(device.Properties.Delta(config.DeviceProps));
                    item.Properties.DeserializeEditable(item.Properties.Delta(config.ItemProps));
                    return item;
                }
                else
                {
                    return device.PromptToConfigure(hwnd);
                }
            }
            else if (device.Version == WiaVersion.Wia10)
            {
                // In WIA 1.0, the root device only has a single child, "Scan"
                // https://docs.microsoft.com/en-us/windows-hardware/drivers/image/wia-scanner-tree
                return device.GetSubItems().First();
            }
            else
            {
                // In WIA 2.0, the root device may have multiple children, i.e. "Flatbed" and "Feeder"
                // https://docs.microsoft.com/en-us/windows-hardware/drivers/image/non-duplex-capable-document-feeder
                // The "Feeder" child may also have a pair of children (for front/back sides with duplex)
                // https://docs.microsoft.com/en-us/windows-hardware/drivers/image/simple-duplex-capable-document-feeder
                var items = device.GetSubItems();
                var preferredItemName = ScanProfile.PaperSource == ScanSource.Glass ? "Flatbed" : "Feeder";
                return items.FirstOrDefault(x => x.Name() == preferredItemName) ?? items.First();
            }
        }

        private void ConfigureProps(WiaDevice device, WiaItem item)
        {
            if (ScanProfile.UseNativeUI)
            {
                return;
            }

            if (ScanProfile.PaperSource != ScanSource.Glass)
            {
                if (device.Version == WiaVersion.Wia10)
                {
                    device.SetProperty(WiaPropertyId.DPS_PAGES, 1);
                }
                else
                {
                    item.SetProperty(WiaPropertyId.IPS_PAGES, 0);
                }
            }

            if (device.Version == WiaVersion.Wia10)
            {
                switch (ScanProfile.PaperSource)
                {
                    case ScanSource.Glass:
                        device.SetProperty(WiaPropertyId.DPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.FLATBED);
                        break;
                    case ScanSource.Feeder:
                        device.SetProperty(WiaPropertyId.DPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.FEEDER);
                        break;
                    case ScanSource.Duplex:
                        device.SetProperty(WiaPropertyId.DPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.FEEDER | WiaPropertyValue.DUPLEX);
                        break;
                }
            }
            else
            {
                switch (ScanProfile.PaperSource)
                {
                    case ScanSource.Feeder:
                        item.SetProperty(WiaPropertyId.IPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.FRONT_ONLY);
                        break;
                    case ScanSource.Duplex:
                        item.SetProperty(WiaPropertyId.IPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.DUPLEX);
                        break;
                }
            }

            switch (ScanProfile.BitDepth)
            {
                case ScanBitDepth.Grayscale:
                    item.SetProperty(WiaPropertyId.IPA_DATATYPE, 2);
                    break;
                case ScanBitDepth.C24Bit:
                    item.SetProperty(WiaPropertyId.IPA_DATATYPE, 3);
                    break;
                case ScanBitDepth.BlackWhite:
                    item.SetProperty(WiaPropertyId.IPA_DATATYPE, 0);
                    break;
            }

            int xRes = ScanProfile.Resolution.ToIntDpi();
            int yRes = xRes;
            item.SetPropertyClosest(WiaPropertyId.IPS_XRES, ref xRes);
            item.SetPropertyClosest(WiaPropertyId.IPS_YRES, ref yRes);

            PageDimensions pageDimensions = ScanProfile.PageSize.PageDimensions() ?? ScanProfile.CustomPageSize;
            if (pageDimensions == null)
            {
                throw new InvalidOperationException("No page size specified");
            }
            int pageWidth = pageDimensions.WidthInThousandthsOfAnInch() * xRes / 1000;
            int pageHeight = pageDimensions.HeightInThousandthsOfAnInch() * yRes / 1000;

            int horizontalSize, verticalSize;
            if (device.Version == WiaVersion.Wia10)
            {
                horizontalSize =
                    (int)device.Properties[ScanProfile.PaperSource == ScanSource.Glass
                        ? WiaPropertyId.DPS_HORIZONTAL_BED_SIZE
                        : WiaPropertyId.DPS_HORIZONTAL_SHEET_FEED_SIZE].Value;
                verticalSize =
                    (int)device.Properties[ScanProfile.PaperSource == ScanSource.Glass
                        ? WiaPropertyId.DPS_VERTICAL_BED_SIZE
                        : WiaPropertyId.DPS_VERTICAL_SHEET_FEED_SIZE].Value;
            }
            else
            {
                horizontalSize = (int)item.Properties[WiaPropertyId.IPS_MAX_HORIZONTAL_SIZE].Value;
                verticalSize = (int)item.Properties[WiaPropertyId.IPS_MAX_VERTICAL_SIZE].Value;
            }

            int pagemaxwidth = horizontalSize * xRes / 1000;
            int pagemaxheight = verticalSize * yRes / 1000;

            int horizontalPos = 0;
            if (ScanProfile.PageAlign == ScanHorizontalAlign.Center)
                horizontalPos = (pagemaxwidth - pageWidth) / 2;
            else if (ScanProfile.PageAlign == ScanHorizontalAlign.Left)
                horizontalPos = (pagemaxwidth - pageWidth);

            pageWidth = pageWidth < pagemaxwidth ? pageWidth : pagemaxwidth;
            pageHeight = pageHeight < pagemaxheight ? pageHeight : pagemaxheight;

            if (ScanProfile.WiaOffsetWidth)
            {
                item.SetProperty(WiaPropertyId.IPS_XEXTENT, pageWidth + horizontalPos);
                item.SetProperty(WiaPropertyId.IPS_XPOS, horizontalPos);
            }
            else
            {
                item.SetProperty(WiaPropertyId.IPS_XEXTENT, pageWidth);
                item.SetProperty(WiaPropertyId.IPS_XPOS, horizontalPos);
            }
            item.SetProperty(WiaPropertyId.IPS_YEXTENT, pageHeight);

            if (!ScanProfile.BrightnessContrastAfterScan)
            {
                item.SetPropertyRange(WiaPropertyId.IPS_CONTRAST, ScanProfile.Contrast, -1000, 1000);
                item.SetPropertyRange(WiaPropertyId.IPS_BRIGHTNESS, ScanProfile.Brightness, -1000, 1000);
            }
        }
    }
}
