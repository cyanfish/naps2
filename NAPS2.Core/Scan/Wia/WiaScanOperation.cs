using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
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

        public WiaScanOperation(ScannedImageHelper scannedImageHelper, IBlankDetector blankDetector, IWorkerServiceFactory workerServiceFactory)
        {
            this.scannedImageHelper = scannedImageHelper;
            this.blankDetector = blankDetector;
            this.workerServiceFactory = workerServiceFactory;
            AllowCancel = true;
            AllowBackground = true;
        }

        public Exception ScanException { get; private set; }

        private ScanProfile ScanProfile { get; set; }

        private ScanParams ScanParams { get; set; }

        private IWin32Window DialogParent { get; set; }

        public bool Start(ScanProfile scanProfile, ScanParams scanParams, IWin32Window dialogParent, ScannedImageSource.Concrete source)
        {
            ScanProfile = scanProfile;
            ScanParams = scanParams;
            DialogParent = dialogParent;
            ProgressTitle = ScanProfile.Device?.Name;
            Status = new OperationStatus
            {
                StatusText = ScanProfile.PaperSource == ScanSource.Glass
                    ? MiscResources.AcquiringData
                    : string.Format(MiscResources.ScanProgressPage, 1),
                MaxProgress = 100,
                ProgressType = OperationProgressType.BarOnly
            };

            RunAsync(() =>
            {
                try
                {
                    try
                    {
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
            });

            return true;
        }

        private void Scan(ScannedImageSource.Concrete source)
        {
            using (var deviceManager = new WiaDeviceManager(ScanProfile.WiaVersion))
            using (var device = deviceManager.FindDevice(ScanProfile.Device.ID))
            using (var item = GetItem(device))
            using (var transfer = item?.StartTransfer())
            {
                if (transfer == null)
                {
                    return;
                }

                if (ScanProfile.PaperSource != ScanSource.Glass && !device.SupportsFeeder())
                {
                    throw new NoFeederSupportException();
                }

                if (ScanProfile.PaperSource == ScanSource.Duplex && !device.SupportsDuplex())
                {
                    throw new NoDuplexSupportException();
                }

                ProgressTitle = device.Name();
                InvokeStatusChanged();

                // TODO: Test 64-bit wia and figure out if I need the worker

                // TODO: Delete the delay/retry options in ScanProfile (but make sure not to break xml parsing)

                // TODO: Catch and log E_INVALIDARG when setting properties

                ConfigureProps(device, item);
                // TODO: Format BMP "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}"

                // TODO: For WIA native UI, use WiaDevMgr::GetImageDlg (and SelectDeviceDlgID for device selection)

                // TODO: Progress form (operation?)
                // TODO: Use ScanParams.Modal as needed

                int pageNumber = 1;
                transfer.PageScanned += (sender, args) =>
                {
                    try
                    {
                        using (args.Stream)
                        using (Image output = Image.FromStream(args.Stream))
                        using (var result = scannedImageHelper.PostProcessStep1(output, ScanProfile))
                        {
                            if (blankDetector.ExcludePage(result, ScanProfile))
                            {
                                return;
                            }

                            ScanBitDepth bitDepth = ScanProfile.UseNativeUI ? ScanBitDepth.C24Bit : ScanProfile.BitDepth;
                            var image = new ScannedImage(result, bitDepth, ScanProfile.MaxQuality, ScanProfile.Quality);
                            scannedImageHelper.PostProcessStep2(image, result, ScanProfile, ScanParams, pageNumber++);
                            string tempPath = scannedImageHelper.SaveForBackgroundOcr(result, ScanParams);
                            scannedImageHelper.RunBackgroundOcr(image, ScanParams, tempPath);
                            source.Put(image);

                            if (ScanProfile.PaperSource != ScanSource.Glass)
                            {
                                Status.StatusText = string.Format(MiscResources.ScanProgressPage, pageNumber);
                                Status.CurrentProgress = 0;
                                InvokeStatusChanged();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ScanException = e;
                    }
                };
                transfer.Progress += (sender, args) =>
                {
                    Status.CurrentProgress = args.Percent;
                    InvokeStatusChanged();
                };
                using (CancelToken.Register(transfer.Cancel))
                {
                    transfer.Download();
                }
            }
        }

        private WiaItem GetItem(WiaDevice device)
        {
            if (ScanProfile.UseNativeUI)
            {
                // TODO: For native UI, (a) always use WIA 1.0, probably (b) use the worker to run the thing as 32-bit
                // TODO: Come to think of it, that's exactly the same behaviour as wiaaut. Maybe just use that.
                // TODO: Upon further reflection (a) use WIA 2.0 by default since it works with all scanners. That's more important than slight weirdness.
                // TODO: (b) For WIA 1.0 worker use, we can do the transfer locally and just use the worker to serialize an item id (?) + map of property values.
                // TODO: The worker can enumerate all properties, then show the window (using the serialized window handle), then do a diff and send over all
                // TODO: different property id/value pairs to be set on the client. Integers only.
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
            
            if (ScanProfile.PaperSource != ScanSource.Glass && device.SupportsFeeder())
            {
                if (device.Version == WiaVersion.Wia10)
                {
                    // TODO: When implementing in C++, make sure we transfer as many pages as needed
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
                        item.SetProperty(WiaPropertyId.IPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.DUPLEX | WiaPropertyValue.FRONT_FIRST);
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

            // TODO: Need to somehow find the list of valid resolutions, and pick the closest one to the user selection.
            int resolution = ScanProfile.Resolution.ToIntDpi();
            item.SetProperty(WiaPropertyId.IPS_YRES, resolution);
            item.SetProperty(WiaPropertyId.IPS_XRES, resolution);

            PageDimensions pageDimensions = ScanProfile.PageSize.PageDimensions() ?? ScanProfile.CustomPageSize;
            if (pageDimensions == null)
            {
                throw new InvalidOperationException("No page size specified");
            }
            int pageWidth = pageDimensions.WidthInThousandthsOfAnInch() * resolution / 1000;
            int pageHeight = pageDimensions.HeightInThousandthsOfAnInch() * resolution / 1000;

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

            int pagemaxwidth = horizontalSize * resolution / 1000;
            int pagemaxheight = verticalSize * resolution / 1000;

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
