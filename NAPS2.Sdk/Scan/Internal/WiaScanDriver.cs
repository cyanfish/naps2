using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images.Storage;
using NAPS2.Logging;
using NAPS2.Remoting.Worker;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Wia;
using NAPS2.Scan.Wia.Native;
using NAPS2.Util;

namespace NAPS2.Scan.Internal
{
    internal class WiaScanDriver : IScanDriver
    {
        private readonly ImageContext imageContext;
        private readonly IWorkerFactory workerFactory;

        public WiaScanDriver(ImageContext imageContext)
            : this(imageContext, WorkerFactory.Default)
        {
        }

        public WiaScanDriver(ImageContext imageContext, IWorkerFactory workerFactory)
        {
            this.imageContext = imageContext;
            this.workerFactory = workerFactory;
        }

        public Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
        {
            return Task.Run(() =>
            {
                using (var deviceManager = new WiaDeviceManager(options.WiaOptions.WiaVersion))
                {
                    return deviceManager.GetDeviceInfos().Select(deviceInfo =>
                    {
                        using (deviceInfo)
                        {
                            return new ScanDevice(deviceInfo.Id(), deviceInfo.Name());
                        }
                    }).ToList();
                }
            });
        }

        public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IImage> callback)
        {
            return Task.Run(() =>
            {
                var context = new WiaScanContext(imageContext, workerFactory)
                {
                    Options = options,
                    ScanEvents = scanEvents,
                    CancelToken = cancelToken,
                    Callback = callback
                };
                try
                {
                    try
                    {
                        context.Scan(options.WiaOptions.WiaVersion);
                    }
                    catch (WiaException e) when
                    (e.ErrorCode == Hresult.E_INVALIDARG &&
                     options.WiaOptions.WiaVersion == WiaVersion.Default &&
                     NativeWiaObject.DefaultWiaVersion == WiaVersion.Wia20
                     && !options.UseNativeUI)
                    {
                        Debug.WriteLine("Falling back to WIA 1.0 due to E_INVALIDARG");
                        context.Scan(WiaVersion.Wia10);
                    }
                }
                catch (WiaException e)
                {
                    WiaScanErrors.ThrowDeviceError(e);
                }
            });
        }

        private class WiaScanContext
        {
            private readonly ImageContext imageContext;
            private readonly IWorkerFactory workerFactory;

            public WiaScanContext(ImageContext imageContext, IWorkerFactory workerFactory)
            {
                this.imageContext = imageContext;
                this.workerFactory = workerFactory;
            }

            public ScanOptions Options { get; set; }

            public CancellationToken CancelToken { get; set; }

            public IScanEvents ScanEvents { get; set; }

            public Action<IImage> Callback { get; set; }

            public void Scan(WiaVersion wiaVersion)
            {
                using (var deviceManager = new WiaDeviceManager(wiaVersion))
                using (var device = deviceManager.FindDevice(Options.Device.ID))
                {
                    if (device.Version == WiaVersion.Wia20 && Options.UseNativeUI)
                    {
                        DoWia20NativeTransfer(deviceManager, device);
                        return;
                    }

                    using (var item = GetItem(device))
                    {
                        if (item == null)
                        {
                            return;
                        }

                        DoTransfer(device, item);
                    }
                }
            }

            private void DoWia20NativeTransfer(WiaDeviceManager deviceManager, WiaDevice device)
            {
                // WIA 2.0 doesn't support normal transfers with native UI.
                // Instead we need to have it write the scans to a set of files and load those.

                var paths = deviceManager.PromptForImage(Options.DialogParent, device);

                if (paths == null)
                {
                    return;
                }

                try
                {
                    foreach (var path in paths)
                    {
                        using (var stream = new FileStream(path, FileMode.Open))
                        {
                            foreach (var image in imageContext.ImageFactory.DecodeMultiple(stream, Path.GetExtension(path), out _))
                            {
                                using (image)
                                {
                                    // TODO: Might still need to do some work on ownership for in-memory ScannedImage storage
                                    Callback(image);
                                }
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

            private void DoTransfer(WiaDevice device, WiaItem item)
            {
                if (Options.PaperSource != PaperSource.Flatbed && !device.SupportsFeeder())
                {
                    throw new NoFeederSupportException();
                }
                if (Options.PaperSource == PaperSource.Duplex && !device.SupportsDuplex())
                {
                    throw new NoDuplexSupportException();
                }

                ConfigureProps(device, item);

                using (var transfer = item.StartTransfer())
                {
                    Exception scanException = null;
                    transfer.PageScanned += (sender, args) =>
                    {
                        try
                        {
                            using (args.Stream)
                            using (var image = imageContext.ImageFactory.Decode(args.Stream, ".bmp"))
                            {
                                Callback(image);
                            }
                        }
                        catch (Exception e)
                        {
                            e.PreserveStackTrace();
                            scanException = e;
                        }
                    };
                    transfer.Progress += (sender, args) => ScanEvents.PageProgress(args.Percent / 100.0);
                    using (CancelToken.Register(transfer.Cancel))
                    {
                        ScanEvents.PageStart();
                        transfer.Download();

                        if (device.Version == WiaVersion.Wia10 && Options.PaperSource != PaperSource.Flatbed)
                        {
                            // For WIA 1.0 feeder scans, we need to repeatedly call Download until WIA_ERROR_PAPER_EMPTY is received.
                            try
                            {
                                while (!CancelToken.IsCancellationRequested && scanException == null)
                                {
                                    ScanEvents.PageStart();
                                    transfer.Download();
                                }
                            }
                            catch (WiaException e) when (e.ErrorCode == WiaErrorCodes.PAPER_EMPTY)
                            {
                            }
                        }
                    }
                    if (scanException != null)
                    {
                        throw scanException;
                    }
                }
            }

            private WiaItem GetItem(WiaDevice device)
            {
                if (Options.UseNativeUI)
                {
                    bool useWorker = Environment.Is64BitProcess && device.Version == WiaVersion.Wia10;
                    if (useWorker)
                    {
                        WiaConfiguration config;
                        using (var worker = workerFactory.Create())
                        {
                            config = worker.Service.Wia10NativeUI(device.Id(), Options.DialogParent);
                        }
                        var item = device.FindSubItem(config.ItemName);
                        device.Properties.DeserializeEditable(device.Properties.Delta(config.DeviceProps));
                        item.Properties.DeserializeEditable(item.Properties.Delta(config.ItemProps));
                        return item;
                    }
                    else
                    {
                        return device.PromptToConfigure(Options.DialogParent);
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
                    var preferredItemName = Options.PaperSource == PaperSource.Flatbed ? "Flatbed" : "Feeder";
                    return items.FirstOrDefault(x => x.Name() == preferredItemName) ?? items.First();
                }
            }

            private void ConfigureProps(WiaDevice device, WiaItem item)
            {
                if (Options.UseNativeUI)
                {
                    return;
                }

                if (Options.PaperSource != PaperSource.Flatbed)
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
                    switch (Options.PaperSource)
                    {
                        case PaperSource.Flatbed:
                            device.SetProperty(WiaPropertyId.DPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.FLATBED);
                            break;
                        case PaperSource.Feeder:
                            device.SetProperty(WiaPropertyId.DPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.FEEDER);
                            break;
                        case PaperSource.Duplex:
                            device.SetProperty(WiaPropertyId.DPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.FEEDER | WiaPropertyValue.DUPLEX);
                            break;
                    }
                }
                else
                {
                    switch (Options.PaperSource)
                    {
                        case PaperSource.Feeder:
                            item.SetProperty(WiaPropertyId.IPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.FRONT_ONLY);
                            break;
                        case PaperSource.Duplex:
                            item.SetProperty(WiaPropertyId.IPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.DUPLEX | WiaPropertyValue.FRONT_FIRST);
                            break;
                    }
                }

                switch (Options.BitDepth)
                {
                    case BitDepth.Grayscale:
                        item.SetProperty(WiaPropertyId.IPA_DATATYPE, 2);
                        break;
                    case BitDepth.Color:
                        item.SetProperty(WiaPropertyId.IPA_DATATYPE, 3);
                        break;
                    case BitDepth.BlackAndWhite:
                        item.SetProperty(WiaPropertyId.IPA_DATATYPE, 0);
                        break;
                }

                int xRes = Options.Dpi;
                int yRes = Options.Dpi;
                item.SetPropertyClosest(WiaPropertyId.IPS_XRES, ref xRes);
                item.SetPropertyClosest(WiaPropertyId.IPS_YRES, ref yRes);

                int pageWidth = Options.PageSize.WidthInThousandthsOfAnInch * xRes / 1000;
                int pageHeight = Options.PageSize.HeightInThousandthsOfAnInch * yRes / 1000;

                int horizontalSize, verticalSize;
                if (device.Version == WiaVersion.Wia10)
                {
                    horizontalSize =
                        (int)device.Properties[Options.PaperSource == PaperSource.Flatbed
                            ? WiaPropertyId.DPS_HORIZONTAL_BED_SIZE
                            : WiaPropertyId.DPS_HORIZONTAL_SHEET_FEED_SIZE].Value;
                    verticalSize =
                        (int)device.Properties[Options.PaperSource == PaperSource.Flatbed
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
                if (Options.PageAlign == HorizontalAlign.Center)
                    horizontalPos = (pagemaxwidth - pageWidth) / 2;
                else if (Options.PageAlign == HorizontalAlign.Left)
                    horizontalPos = (pagemaxwidth - pageWidth);

                pageWidth = pageWidth < pagemaxwidth ? pageWidth : pagemaxwidth;
                pageHeight = pageHeight < pagemaxheight ? pageHeight : pagemaxheight;

                if (Options.WiaOptions.OffsetWidth)
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

                if (!Options.BrightnessContrastAfterScan)
                {
                    item.SetPropertyRange(WiaPropertyId.IPS_CONTRAST, Options.Contrast, -1000, 1000);
                    item.SetPropertyRange(WiaPropertyId.IPS_BRIGHTNESS, Options.Brightness, -1000, 1000);
                }
            }
        }
    }
}