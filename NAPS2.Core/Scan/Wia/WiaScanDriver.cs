using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Platform;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.Scan.Wia.Native;
using NAPS2.Util;
using NAPS2.WinForms;
using NAPS2.Worker;

namespace NAPS2.Scan.Wia
{
    public class WiaScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "wia";

        private readonly IBlankDetector blankDetector;
        private readonly ScannedImageHelper scannedImageHelper;
        private readonly IFormFactory formFactory;
        private readonly IWorkerServiceFactory workerServiceFactory;

        public WiaScanDriver(IBlankDetector blankDetector, ScannedImageHelper scannedImageHelper, IFormFactory formFactory, IWorkerServiceFactory workerServiceFactory)
            : base(formFactory)
        {
            this.blankDetector = blankDetector;
            this.scannedImageHelper = scannedImageHelper;
            this.formFactory = formFactory;
            this.workerServiceFactory = workerServiceFactory;
        }

        public override string DriverName => DRIVER_NAME;

        public override bool IsSupported => PlatformCompat.System.IsWiaDriverSupported;

        protected override List<ScanDevice> GetDeviceListInternal()
        {
            using (var deviceManager = new WiaDeviceManager(ScanProfile.WiaVersion ?? WiaVersion.Wia10))
            {
                return deviceManager.GetDeviceInfos().Select(x =>
                {
                    using (x)
                    {
                        return new ScanDevice(x.Id(), x.Name());
                    }
                }).ToList();
            }
        }

        protected override async Task ScanInternal(ScannedImageSource.Concrete source)
        {
            try
            {
                using (var deviceManager = new WiaDeviceManager(ScanProfile.WiaVersion ?? WiaVersion.Wia10))
                using (var device = deviceManager.FindDevice(ScanProfile.Device.ID))
                using (var item = GetItem(device))
                using (var transfer = item.StartTransfer())
                {
                    if (ScanProfile.PaperSource != ScanSource.Glass && !device.SupportsFeeder())
                    {
                        throw new NoFeederSupportException();
                    }

                    if (ScanProfile.PaperSource == ScanSource.Duplex && !device.SupportsDuplex())
                    {
                        throw new NoDuplexSupportException();
                    }

                    // TODO: Test 64-bit wia and figure out if I need the worker

                    // TODO: Delete the delay/retry options in ScanProfile (but make sure not to break xml parsing)

                    // TODO: Props subtype/range. Do we need to manually validate before writing?
                    // TODO: Seems like the COM uses WIA 1.0. Do I need to support both? Can I just use 1.0, is that okay? Probably I should have both for compatibility purposes...
                    ConfigureDeviceProps(device);
                    ConfigureItemProps(device, item);
                    // TODO: Format BMP "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}"

                    // TODO: Arrrggg... WIA native UI is an option I'm supposed to support.
                    if (device.Version == WiaVersion.Wia10)
                    {
                        device.Properties[WiaPropertyId.DPS_PAGES].Value = 0;
                    }
                    else
                    {
                        item.Properties[WiaPropertyId.IPS_PAGES].Value = 0;
                    }

                    // TODO: Progress form (operation?)
                    // TODO: Use ScanParams.Modal as needed

                    int pageNumber = 1;
                    transfer.PageScanned += (sender, args) =>
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
                        }
                    };
                    using (CancelToken.Register(transfer.Cancel))
                    {
                        // TODO: Maybe wrap the entire thing in a task; some parts might hang
                        await Task.Factory.StartNew(transfer.Download, TaskCreationOptions.LongRunning);
                    }
                }
            }
            catch (WiaException e)
            {
                ThrowDeviceError(e);
            }
        }

        private WiaItem GetItem(WiaDevice device)
        {
            if (device.Version == WiaVersion.Wia10)
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

        private void ConfigureItemProps(WiaDevice device, WiaItem item)
        {
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

            int maxResolution = Math.Min(item.GetPropertyMax(WiaPropertyId.IPS_YRES), item.GetPropertyMax(WiaPropertyId.IPS_XRES));
            int resolution = Math.Min(ScanProfile.Resolution.ToIntDpi(), maxResolution);
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

        private void ConfigureDeviceProps(WiaDevice device)
        {
            if (ScanProfile.PaperSource != ScanSource.Glass && device.SupportsFeeder())
            {
                device.SetProperty(WiaPropertyId.DPS_PAGES, 0);
            }

            switch (ScanProfile.PaperSource)
            {
                // TODO: Also use this to select the device
                case ScanSource.Glass:
                    device.SetProperty(WiaPropertyId.DPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.FLATBED);
                    break;
                case ScanSource.Feeder:
                    device.SetProperty(WiaPropertyId.DPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.FEEDER);
                    break;
                case ScanSource.Duplex:
                    device.SetProperty(WiaPropertyId.DPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.DUPLEX);
                    break;
            }
        }

        private void ThrowDeviceError(WiaException e)
        {
            // TODO: Figure out what error code FindDevice returns and throw DeviceNotFoundException
            if (e.ErrorCode == WiaErrorCodes.NO_DEVICE_FOUND)
            {
                throw new NoDevicesFoundException();
            }
            if (e.ErrorCode == WiaErrorCodes.OUT_OF_PAPER)
            {
                throw new NoPagesException();
            }
            if (e.ErrorCode == WiaErrorCodes.OFFLINE)
            {
                throw new DeviceException(MiscResources.DeviceOffline);
            }
            if (e.ErrorCode == WiaErrorCodes.BUSY)
            {
                throw new DeviceException(MiscResources.DeviceBusy);
            }
            if (e.ErrorCode == WiaErrorCodes.COVER_OPEN)
            {
                throw new DeviceException(MiscResources.DeviceCoverOpen);
            }
            if (e.ErrorCode == WiaErrorCodes.PAPER_JAM)
            {
                throw new DeviceException(MiscResources.DevicePaperJam);
            }
            if (e.ErrorCode == WiaErrorCodes.WARMING_UP)
            {
                throw new DeviceException(MiscResources.DeviceWarmingUp);
            }
            throw new ScanDriverUnknownException(e);
        }
    }
}
