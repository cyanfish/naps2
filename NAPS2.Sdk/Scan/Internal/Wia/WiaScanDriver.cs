#if !MAC
using System.Threading;
using Microsoft.Extensions.Logging;
using NAPS2.Remoting.Worker;
using NAPS2.Scan.Exceptions;
using NAPS2.Wia;

namespace NAPS2.Scan.Internal.Wia;

#if NET6_0_OR_GREATER
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
internal class WiaScanDriver : IScanDriver
{
    private readonly ScanningContext _scanningContext;

    public WiaScanDriver(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback)
    {
        return Task.Run(() =>
        {
            using var deviceManager = new WiaDeviceManager((WiaVersion) options.WiaOptions.WiaApiVersion);
            foreach (var deviceInfo in deviceManager.GetDeviceInfos())
            {
                using (deviceInfo)
                {
                    callback(new ScanDevice(deviceInfo.Id(), deviceInfo.Name()));
                }
            }
        });
    }

    public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<IMemoryImage> callback)
    {
        return Task.Run(async () =>
        {
            var context = new WiaScanContext(_scanningContext, options, cancelToken, scanEvents, callback);
            try
            {
                var version = (WiaVersion) options.WiaOptions.WiaApiVersion;
                try
                {
                    await context.Scan(version);
                }
                catch (WiaException e) when
                    (e.ErrorCode == Hresult.E_INVALIDARG &&
                     version == WiaVersion.Default &&
                     NativeWiaObject.DefaultWiaVersion == WiaVersion.Wia20
                     && !options.UseNativeUI)
                {
                    Debug.WriteLine("Falling back to WIA 1.0 due to E_INVALIDARG");
                    await context.Scan(WiaVersion.Wia10);
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
        private readonly ILogger _logger;
        private readonly ScanOptions _options;
        private readonly CancellationToken _cancelToken;
        private readonly IScanEvents _scanEvents;
        private readonly Action<IMemoryImage> _callback;

        public WiaScanContext(ScanningContext scanningContext, ScanOptions options, CancellationToken cancelToken,
            IScanEvents scanEvents, Action<IMemoryImage> callback)
        {
            _scanningContext = scanningContext;
            _logger = scanningContext.Logger;
            _options = options;
            _cancelToken = cancelToken;
            _scanEvents = scanEvents;
            _callback = callback;
        }

        public async Task Scan(WiaVersion wiaVersion)
        {
            using var deviceManager = new WiaDeviceManager(wiaVersion);
            using var device = deviceManager.FindDevice(_options.Device!.ID!);
            if (device.Version == WiaVersion.Wia20 && _options.UseNativeUI)
            {
                await DoWia20NativeTransfer(deviceManager, device);
                return;
            }

            if (_options.PaperSource == PaperSource.Auto)
            {
                // Default to flatbed if supported (or if both support checks fail)
                _options.PaperSource = SupportsFlatbed(device) || !device.SupportsFeeder()
                    ? PaperSource.Flatbed
                    : PaperSource.Feeder;
            }

            using var item = GetItem(device);
            if (item == null)
            {
                return;
            }

            DoTransfer(device, item);
        }

        private async Task DoWia20NativeTransfer(WiaDeviceManager deviceManager, WiaDevice device)
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
                    await foreach (var image in _scanningContext.ImageContext.LoadFrames(path))
                    {
                        using (image)
                        {
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
                        _logger.LogError(e, "Error deleting WIA 2.0 native transferred file");
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
                    using var stream = args.Stream;
                    if (stream.Length == 0)
                    {
                        _logger.LogError("Ignoring empty stream from WIA");
                        return;
                    }
                    using var image = _scanningContext.ImageContext.Load(stream);
                    _callback(image);
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
                // TODO: Need to call PageStart multiple times for feeder (and non-wia-1.0?)
                _scanEvents.PageStart();
                try
                {
                    transfer.Download();
                }
                catch (WiaException e) when (e.ErrorCode == 0x210001)
                {
                    // This error code is undocumented but seems to mean "no more pages" which can be ignored
                }

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

        private static bool SupportsFlatbed(WiaDevice device)
        {
            // TODO: Move this to NAPS2.Wia
            int capabilities = (int) device.Properties[WiaPropertyId.DPS_DOCUMENT_HANDLING_CAPABILITIES].Value;
            return (capabilities & WiaPropertyValue.FLATBED) != 0;
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
                    using (var worker = _scanningContext.CreateWorker(WorkerType.WinX86)!)
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
                    SafeSetProperty(device, WiaPropertyId.DPS_PAGES, 1);
                }
                else
                {
                    SafeSetProperty(item, WiaPropertyId.IPS_PAGES, 0);
                }
            }

            if (device.Version == WiaVersion.Wia10)
            {
                switch (_options.PaperSource)
                {
                    case PaperSource.Flatbed:
                        SafeSetProperty(device, WiaPropertyId.DPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.FLATBED);
                        break;
                    case PaperSource.Feeder:
                        SafeSetProperty(device, WiaPropertyId.DPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.FEEDER);
                        break;
                    case PaperSource.Duplex:
                        SafeSetProperty(device, WiaPropertyId.DPS_DOCUMENT_HANDLING_SELECT,
                            WiaPropertyValue.FEEDER | WiaPropertyValue.DUPLEX);
                        break;
                }
            }
            else
            {
                switch (_options.PaperSource)
                {
                    case PaperSource.Feeder:
                        SafeSetProperty(item, WiaPropertyId.IPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.FRONT_ONLY);
                        break;
                    case PaperSource.Duplex:
                        SafeSetProperty(item, WiaPropertyId.IPS_DOCUMENT_HANDLING_SELECT, WiaPropertyValue.DUPLEX);
                        break;
                }
            }

            switch (_options.BitDepth)
            {
                case BitDepth.Grayscale:
                    SafeSetProperty(item, WiaPropertyId.IPA_DATATYPE, 2);
                    break;
                case BitDepth.Color:
                    SafeSetProperty(item, WiaPropertyId.IPA_DATATYPE, 3);
                    break;
                case BitDepth.BlackAndWhite:
                    SafeSetProperty(item, WiaPropertyId.IPA_DATATYPE, 0);
                    break;
            }

            int xRes = _options.Dpi;
            int yRes = _options.Dpi;
            SafeSetPropertyClosest(item, WiaPropertyId.IPS_XRES, ref xRes);
            SafeSetPropertyClosest(item, WiaPropertyId.IPS_YRES, ref yRes);

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
                SafeSetProperty(item, WiaPropertyId.IPS_XEXTENT, pageWidth + horizontalPos);
                SafeSetProperty(item, WiaPropertyId.IPS_XPOS, horizontalPos);
            }
            else
            {
                SafeSetProperty(item, WiaPropertyId.IPS_XEXTENT, pageWidth);
                SafeSetProperty(item, WiaPropertyId.IPS_XPOS, horizontalPos);
            }
            SafeSetProperty(item, WiaPropertyId.IPS_YEXTENT, pageHeight);

            if (!_options.BrightnessContrastAfterScan)
            {
                SafeSetPropertyRange(item, WiaPropertyId.IPS_CONTRAST, _options.Contrast, -1000, 1000);
                SafeSetPropertyRange(item, WiaPropertyId.IPS_BRIGHTNESS, _options.Brightness, -1000, 1000);
            }
        }

        private void SafeSetProperty(WiaItemBase item, int propId, int value)
        {
            try
            {
                item.SetProperty(propId, value);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error setting property {PropId}", propId);
            }
        }

        private void SafeSetPropertyClosest(WiaItemBase item, int propId, ref int value)
        {
            try
            {
                item.SetPropertyClosest(propId, ref value);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error setting property {PropId}", propId);
            }
        }

        private void SafeSetPropertyRange(WiaItemBase item, int propId, int value, int expectedMin, int expectedMax)
        {
            try
            {
                item.SetPropertyRange(propId, value, expectedMin, expectedMax);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error setting property {PropId}", propId);
            }
        }
    }
}
#endif