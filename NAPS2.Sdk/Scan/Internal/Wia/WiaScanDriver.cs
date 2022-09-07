using System.Threading;
using NAPS2.Scan.Exceptions;
using NAPS2.Wia;

namespace NAPS2.Scan.Internal.Wia;

internal class WiaScanDriver : IScanDriver
{
    private readonly ScanningContext _scanningContext;

    public WiaScanDriver(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        return Task.Run(() =>
        {
            using var deviceManager = new WiaDeviceManager(options.WiaOptions.WiaVersion);
            return deviceManager.GetDeviceInfos().Select(deviceInfo =>
            {
                using (deviceInfo)
                {
                    return new ScanDevice(deviceInfo.Id(), deviceInfo.Name());
                }
            }).ToList();
        });
    }

    public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<IMemoryImage> callback)
    {
        return Task.Run(() =>
        {
            var context = new WiaScanContext(_scanningContext, options, cancelToken, scanEvents, callback);
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
        private readonly ScanningContext _scanningContext;
        private readonly ScanOptions _options;
        private readonly CancellationToken _cancelToken;
        private readonly IScanEvents _scanEvents;
        private readonly Action<IMemoryImage> _callback;

        public WiaScanContext(ScanningContext scanningContext, ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IMemoryImage> callback)
        {
            _scanningContext = scanningContext;
            _options = options;
            _cancelToken = cancelToken;
            _scanEvents = scanEvents;
            _callback = callback;
        }

        public void Scan(WiaVersion wiaVersion)
        {
            using var deviceManager = new WiaDeviceManager(wiaVersion);
            using var device = deviceManager.FindDevice(_options.Device!.ID!);
            if (device.Version == WiaVersion.Wia20 && _options.UseNativeUI)
            {
                DoWia20NativeTransfer(deviceManager, device);
                return;
            }

            using var item = GetItem(device);
            if (item == null)
            {
                return;
            }

            DoTransfer(device, item);
        }

        private void DoWia20NativeTransfer(WiaDeviceManager deviceManager, WiaDevice device)
        {
            // WIA 2.0 doesn't support normal transfers with native UI.
            // Instead we need to have it write the scans to a set of files and load those.

            var paths = deviceManager.PromptForImage(device, _options.DialogParent);

            if (paths == null)
            {
                return;
            }

            try
            {
                foreach (var path in paths)
                {
                    using var stream = new FileStream(path, FileMode.Open);
                    foreach (var image in _scanningContext.ImageContext.LoadFrames(stream, out _))
                    {
                        using (image)
                        {
                            // TODO: Might still need to do some work on ownership for in-memory ScannedImage storage
                            _callback(image);
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
            if (_options.PaperSource != PaperSource.Flatbed && !device.SupportsFeeder())
            {
                throw new NoFeederSupportException();
            }
            if (_options.PaperSource == PaperSource.Duplex && !device.SupportsDuplex())
            {
                throw new NoDuplexSupportException();
            }

            ConfigureProps(device, item);

            using var transfer = item.StartTransfer();
            Exception? scanException = null;
            transfer.PageScanned += (sender, args) =>
            {
                try
                {
                    using (args.Stream)
                    using (var image = _scanningContext.ImageContext.Load(args.Stream))
                    {
                        _callback(image);
                    }
                }
                catch (Exception e)
                {
                    e.PreserveStackTrace();
                    scanException = e;
                }
            };
            transfer.Progress += (sender, args) => _scanEvents.PageProgress(args.Percent / 100.0);
            using (_cancelToken.Register(transfer.Cancel))
            {
                _scanEvents.PageStart();
                transfer.Download();

                if (device.Version == WiaVersion.Wia10 && _options.PaperSource != PaperSource.Flatbed)
                {
                    // For WIA 1.0 feeder scans, we need to repeatedly call Download until WIA_ERROR_PAPER_EMPTY is received.
                    try
                    {
                        while (!_cancelToken.IsCancellationRequested && scanException == null)
                        {
                            _scanEvents.PageStart();
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

        private WiaItem? GetItem(WiaDevice device)
        {
            if (_options.UseNativeUI)
            {
                bool useWorker = Environment.Is64BitProcess && device.Version == WiaVersion.Wia10;
                if (useWorker)
                {
                    if (_scanningContext.WorkerFactory == null)
                    {
                        throw new InvalidOperationException(
                            "ScanningContext.WorkerFactory must be set to use WIA 1.0 Native UI from a 64-bit process");
                    }
                    WiaConfiguration? config;
                    using (var worker = _scanningContext.WorkerFactory.Create())
                    {
                        config = worker.Service.Wia10NativeUI(device.Id(), _options.DialogParent);
                    }
                    if (config == null)
                    {
                        return null;
                    }
                    var item = device.FindSubItem(config.ItemName);
                    device.Properties.DeserializeEditable(device.Properties.Delta(config.DeviceProps));
                    item.Properties.DeserializeEditable(item.Properties.Delta(config.ItemProps));
                    return item;
                }
                else
                {
                    return device.PromptToConfigure(_options.DialogParent);
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
                var preferredItemName = _options.PaperSource == PaperSource.Flatbed ? "Flatbed" : "Feeder";
                return items.FirstOrDefault(x => x.Name() == preferredItemName) ?? items.First();
            }
        }

        private void ConfigureProps(WiaDevice device, WiaItem item)
        {
            if (_options.UseNativeUI)
            {
                return;
            }

            if (_options.PaperSource != PaperSource.Flatbed)
            {
                if (device.Version == WiaVersion.Wia10)
                {
                    device.SafeSetProperty(WiaPropertyId.DPS_PAGES, 1);
                }
                else
                {
                    item.SafeSetProperty(WiaPropertyId.IPS_PAGES, 0);
                }
            }

            if (device.Version == WiaVersion.Wia10)
            {
                switch (_options.PaperSource)
                {
                    case PaperSource.Flatbed:
                        device.SafeSetProperty(WiaPropertyId.DPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.FLATBED);
                        break;
                    case PaperSource.Feeder:
                        device.SafeSetProperty(WiaPropertyId.DPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.FEEDER);
                        break;
                    case PaperSource.Duplex:
                        device.SafeSetProperty(WiaPropertyId.DPS_DOCUMENT_HANDLING_SELECT,
                            WiaPropertyValue.FEEDER | WiaPropertyValue.DUPLEX);
                        break;
                }
            }
            else
            {
                switch (_options.PaperSource)
                {
                    case PaperSource.Feeder:
                        item.SafeSetProperty(WiaPropertyId.IPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.FRONT_ONLY);
                        break;
                    case PaperSource.Duplex:
                        item.SafeSetProperty(WiaPropertyId.IPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.DUPLEX);
                        break;
                }
            }

            switch (_options.BitDepth)
            {
                case BitDepth.Grayscale:
                    item.SafeSetProperty(WiaPropertyId.IPA_DATATYPE, 2);
                    break;
                case BitDepth.Color:
                    item.SafeSetProperty(WiaPropertyId.IPA_DATATYPE, 3);
                    break;
                case BitDepth.BlackAndWhite:
                    item.SafeSetProperty(WiaPropertyId.IPA_DATATYPE, 0);
                    break;
            }

            int xRes = _options.Dpi;
            int yRes = _options.Dpi;
            item.SafeSetPropertyClosest(WiaPropertyId.IPS_XRES, ref xRes);
            item.SafeSetPropertyClosest(WiaPropertyId.IPS_YRES, ref yRes);

            int pageWidth = _options.PageSize!.WidthInThousandthsOfAnInch * xRes / 1000;
            int pageHeight = _options.PageSize.HeightInThousandthsOfAnInch * yRes / 1000;

            int horizontalSize, verticalSize;
            if (device.Version == WiaVersion.Wia10)
            {
                horizontalSize =
                    (int) device.Properties[_options.PaperSource == PaperSource.Flatbed
                        ? WiaPropertyId.DPS_HORIZONTAL_BED_SIZE
                        : WiaPropertyId.DPS_HORIZONTAL_SHEET_FEED_SIZE].Value;
                verticalSize =
                    (int) device.Properties[_options.PaperSource == PaperSource.Flatbed
                        ? WiaPropertyId.DPS_VERTICAL_BED_SIZE
                        : WiaPropertyId.DPS_VERTICAL_SHEET_FEED_SIZE].Value;
            }
            else
            {
                horizontalSize = (int) item.Properties[WiaPropertyId.IPS_MAX_HORIZONTAL_SIZE].Value;
                verticalSize = (int) item.Properties[WiaPropertyId.IPS_MAX_VERTICAL_SIZE].Value;
            }

            int pagemaxwidth = horizontalSize * xRes / 1000;
            int pagemaxheight = verticalSize * yRes / 1000;

            int horizontalPos = 0;
            if (_options.PageAlign == HorizontalAlign.Center)
                horizontalPos = (pagemaxwidth - pageWidth) / 2;
            else if (_options.PageAlign == HorizontalAlign.Left)
                horizontalPos = (pagemaxwidth - pageWidth);

            pageWidth = pageWidth < pagemaxwidth ? pageWidth : pagemaxwidth;
            pageHeight = pageHeight < pagemaxheight ? pageHeight : pagemaxheight;

            if (_options.WiaOptions.OffsetWidth)
            {
                item.SafeSetProperty(WiaPropertyId.IPS_XEXTENT, pageWidth + horizontalPos);
                item.SafeSetProperty(WiaPropertyId.IPS_XPOS, horizontalPos);
            }
            else
            {
                item.SafeSetProperty(WiaPropertyId.IPS_XEXTENT, pageWidth);
                item.SafeSetProperty(WiaPropertyId.IPS_XPOS, horizontalPos);
            }
            item.SafeSetProperty(WiaPropertyId.IPS_YEXTENT, pageHeight);

            if (!_options.BrightnessContrastAfterScan)
            {
                item.SafeSetPropertyRange(WiaPropertyId.IPS_CONTRAST, _options.Contrast, -1000, 1000);
                item.SafeSetPropertyRange(WiaPropertyId.IPS_BRIGHTNESS, _options.Brightness, -1000, 1000);
            }
        }
    }
}