using System.Collections.Immutable;
using System.Threading;
using Microsoft.Extensions.Logging;
using NAPS2.Images.Bitwise;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Internal.Sane.Native;

namespace NAPS2.Scan.Internal.Sane;

internal class SaneScanDriver : IScanDriver
{
    private static string? _customConfigDir;

    private readonly ScanningContext _scanningContext;

    public SaneScanDriver(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;

#if NET6_0_OR_GREATER
        Installation = OperatingSystem.IsMacOS()
            ? new BundledSaneInstallation()
            : File.Exists("/.flatpak-info")
                ? new FlatpakSaneInstallation()
                : new SystemSaneInstallation();
#else
        Installation = null!;
#endif
    }

    private ISaneInstallation Installation { get; }

    public Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback)
    {
        var localIPsTask = options.ExcludeLocalIPs ? LocalIPsHelper.Get() : null;

        void MaybeCallback(SaneDeviceInfo device)
        {
            if (options.ExcludeLocalIPs && GetIP(device) is { } ip && localIPsTask!.Result.Contains(ip))
            {
                return;
            }
            callback(GetScanDevice(device));
        }

        return Task.Run(() =>
        {
            Installation.Initialize();
            string? tempConfigDir = MaybeCreateTempConfigDirForSingleBackend(options.SaneOptions.Backend);
            try
            {
                // TODO: This is crashing after a delay for no apparent reason.
                // That's okay because we're in a worker process, but ideally we could fix it in SANE.
                using var client = new SaneClient(Installation, options.SaneOptions.KeepInitialized);
                // TODO: We can use device.type and .vendor to help pick an icon etc.
                // https://sane-project.gitlab.io/standard/api.html#device-descriptor-type
                if (Installation.CanStreamDevices)
                {
                    client.StreamDevices(MaybeCallback, cancelToken);
                }
                else
                {
                    foreach (var device in client.GetDevices())
                    {
                        MaybeCallback(device);
                    }
                }
            }
            finally
            {
                if (tempConfigDir != null)
                {
                    try
                    {
                        Directory.Delete(tempConfigDir, true);
                    }
                    catch (Exception ex)
                    {
                        _scanningContext.Logger.LogDebug(ex, "Error cleaning up temp SANE config dir");
                    }
                }
            }
        });
    }

    public Task<ScanCaps> GetCaps(ScanOptions options, CancellationToken cancelToken)
    {
        return Task.Run(() =>
        {
            try
            {
                Installation.Initialize();
                using var client = new SaneClient(Installation, options.SaneOptions.KeepInitialized);
                if (cancelToken.IsCancellationRequested) return new ScanCaps();
                _scanningContext.Logger.LogDebug("Opening SANE Device \"{ID}\" for caps", options.Device!.ID);
                using var device = client.OpenDevice(options.Device.ID);
                if (cancelToken.IsCancellationRequested) return new ScanCaps();

                var controller = new SaneOptionController(device, _scanningContext.Logger);

                PerSourceCaps? flatbed = null;
                PerSourceCaps? feeder = null;
                PerSourceCaps? duplex = null;

                if (controller.TrySet(SaneOptionNames.SOURCE, SaneOptionMatchers.Flatbed))
                {
                    flatbed = GetPerSourceCaps(controller);
                }
                if (controller.TrySet(SaneOptionNames.SOURCE, SaneOptionMatchers.Feeder))
                {
                    feeder = GetPerSourceCaps(controller);
                }
                if (controller.TrySet(SaneOptionNames.SOURCE, SaneOptionMatchers.Duplex) ||
                    controller.TrySet(SaneOptionNames.ADF_MODE1, SaneOptionMatchers.Duplex) ||
                    controller.TrySet(SaneOptionNames.ADF_MODE2, SaneOptionMatchers.Duplex))
                {
                    duplex = GetPerSourceCaps(controller);
                }

                return new ScanCaps
                {
                    MetadataCaps = new MetadataCaps
                    {
                        DriverSubtype = GetBackend(options.Device)
                    },
                    PaperSourceCaps = flatbed != null || feeder != null || duplex != null
                        ? new PaperSourceCaps
                        {
                            SupportsFlatbed = flatbed != null,
                            SupportsFeeder = feeder != null,
                            SupportsDuplex = duplex != null
                        }
                        : null,
                    FlatbedCaps = flatbed,
                    FeederCaps = feeder,
                    DuplexCaps = duplex
                };
            }
            catch (SaneException ex)
            {
                switch (ex.Status)
                {
                    case SaneStatus.Good:
                    case SaneStatus.Cancelled:
                        return new ScanCaps();
                    case SaneStatus.DeviceBusy:
                        throw new DeviceBusyException();
                    case SaneStatus.Invalid:
                        _scanningContext.Logger.LogDebug(ex, "Sane invalid error");
                        throw new DeviceOfflineException();
                    case SaneStatus.IoError:
                        throw new DeviceCommunicationException();
                    default:
                        throw new DeviceException($"SANE error: {ex.Status}");
                }
            }
        });
    }

    private PerSourceCaps GetPerSourceCaps(SaneOptionController controller)
    {
        var resOpt = controller.GetOption(SaneOptionNames.RESOLUTION);
        var xResOpt = controller.GetOption(SaneOptionNames.X_RESOLUTION);
        var yResOpt = controller.GetOption(SaneOptionNames.Y_RESOLUTION);
        var resValues = resOpt != null
            ? GetValues(resOpt)
            : GetValues(xResOpt) is { } xValues && GetValues(yResOpt) is { } yValues
                ? xValues.Intersect(yValues).OrderBy(dpi => dpi).ToImmutableList()
                : null;

        var scanAreaController = new SaneScanAreaController(controller);
        var (minX, minY, maxX, maxY) = scanAreaController.GetBounds();

        return new PerSourceCaps
        {
            BitDepthCaps = controller.GetOption(SaneOptionNames.MODE) != null
                ? new BitDepthCaps
                {
                    SupportsColor = controller.TrySet(SaneOptionNames.MODE, SaneOptionMatchers.Color),
                    SupportsGrayscale = controller.TrySet(SaneOptionNames.MODE, SaneOptionMatchers.Grayscale),
                    SupportsBlackAndWhite = controller.TrySet(SaneOptionNames.MODE, SaneOptionMatchers.BlackAndWhite)
                }
                : null,
            DpiCaps = new DpiCaps
            {
                Values = resValues
            },
            PageSizeCaps = new PageSizeCaps
            {
                ScanArea = scanAreaController.CanSetArea
                    ? new PageSize((decimal) (maxX - minX), (decimal) (maxY - minY), PageSizeUnit.Millimetre)
                    : null
            }
        };
    }

    private ImmutableList<int>? GetValues(SaneOption? option)
    {
        if (option == null) return null;
        if (option.WordList != null)
        {
            return option.WordList.Select(x => (int) x).ToImmutableList();
        }
        if (option.Range != null)
        {
            return DpiCaps.ForRange((int) option.Range.Min, (int) option.Range.Max, (int) option.Range.Quant).Values;
        }
        return null;
    }

    private static ScanDevice GetScanDevice(SaneDeviceInfo device) =>
        new(Driver.Sane, device.Name, GetName(device));

    private static string GetName(SaneDeviceInfo device)
    {
        var backend = GetBackend(device.Name);
        // Special cases for sane-escl and sane-airscan.
        if (backend == "escl")
        {
            // We include the vendor as it's excluded from the model, and we include the full name instead of
            // just the backend as that has the IP address.
            return $"{device.Vendor} {device.Model} ({device.Name})";
        }
        if (backend == "airscan")
        {
            // We include the device type which has the IP address.
            return $"{device.Model} ({backend}:{device.Type})";
        }
        return $"{device.Model} ({backend})";
    }

    private string? GetIP(SaneDeviceInfo device)
    {
        var backend = GetBackend(device.Name);
        if (backend == "escl")
        {
            // Name is in the form "escl:http://xx.xx.xx.xx:yy"
            var uri = new Uri(device.Name.Substring(device.Name.IndexOf(":", StringComparison.InvariantCulture) + 1));
            return uri.Host;
        }
        if (backend == "airscan")
        {
            // Type is in the form "ip=xx.xx.xx.xx"
            return device.Type.Substring(3);
        }
        return null;
    }

    public static string GetBackend(ScanDevice device) => GetBackend(device.ID);

    private static string GetBackend(string saneDeviceName) => saneDeviceName.Split(':')[0];

    public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<IMemoryImage> callback)
    {
        return Task.Run(() =>
        {
            bool hasAtLeastOneImage = false;
            try
            {
                Installation.Initialize();
                using var client = new SaneClient(Installation, options.SaneOptions.KeepInitialized);
                if (cancelToken.IsCancellationRequested) return;
                _scanningContext.Logger.LogDebug("Opening SANE Device \"{ID}\"", options.Device!.ID);
                using var device = client.OpenDevice(options.Device.ID);
                if (cancelToken.IsCancellationRequested) return;
                var optionData = SetOptions(device, options);
                var cancelOnce = new Once(device.Cancel);
                cancelToken.Register(cancelOnce.Run);
                try
                {
                    if (!optionData.IsFeeder)
                    {
                        var image = ScanPage(device, scanEvents, optionData) ??
                                    throw new DeviceException("SANE expected image");
                        callback(image);
                    }
                    else
                    {
                        while (ScanPage(device, scanEvents, optionData) is { } image)
                        {
                            hasAtLeastOneImage = true;
                            callback(image);
                        }
                    }
                }
                finally
                {
                    cancelOnce.Run();
                }
            }
            catch (SaneException ex)
            {
                switch (ex.Status)
                {
                    case SaneStatus.Good:
                    case SaneStatus.Cancelled:
                        return;
                    case SaneStatus.NoDocs:
                        if (!hasAtLeastOneImage)
                        {
                            throw new DeviceFeederEmptyException();
                        }

                        break;
                    case SaneStatus.DeviceBusy:
                        throw new DeviceBusyException();
                    case SaneStatus.Invalid:
                        // TODO: Maybe not always correct? e.g. when setting options
                        throw new DeviceOfflineException();
                    case SaneStatus.Jammed:
                        throw new DevicePaperJamException();
                    case SaneStatus.CoverOpen:
                        throw new DeviceCoverOpenException();
                    case SaneStatus.IoError:
                        throw new DeviceCommunicationException();
                    default:
                        throw new DeviceException($"SANE error: {ex.Status}");
                }
            }
        });
    }

    private string? MaybeCreateTempConfigDirForSingleBackend(string? backendName)
    {
        if (string.IsNullOrEmpty(backendName))
        {
            return null;
        }
        if (!Directory.Exists(Installation.DefaultConfigDir))
        {
            // Non-typical SANE installation where we don't know the config dir and can't do this optimization
            return null;
        }
        if (_customConfigDir == null)
        {
            // SANE caches the SANE_CONFIG_DIR environment variable process-wide, which means that we can't willy-nilly
            // change the config dir. However, if we use a static directory name and only create the actual directory
            // when we want to use it, SANE will (without caching) use the directory when it exists, and fall back to
            // the default config dir otherwise.
            _customConfigDir = Path.Combine(_scanningContext.TempFolderPath, Path.GetRandomFileName());
            Installation.SetCustomConfigDir(_customConfigDir);
        }
        // By using a custom config dir with a dll.conf file that only has a single backend specified, we can force SANE
        // to only check that backend
        Directory.CreateDirectory(_customConfigDir);
        // Copy the backend.conf file in case there's any important backend-specific configuration
        var backendConfFile = $"{backendName}.conf";
        if (File.Exists(Path.Combine(Installation.DefaultConfigDir, backendConfFile)))
        {
            File.Copy(
                Path.Combine(Installation.DefaultConfigDir, backendConfFile),
                Path.Combine(_customConfigDir, backendConfFile));
        }
        // Create a dll.conf file with only the single backend name (normally it's all backends, one per line)
        File.WriteAllText(Path.Combine(_customConfigDir, "dll.conf"), backendName);
        // Create an empty dll.d dir so SANE doesn't use the default one
        Directory.CreateDirectory(Path.Combine(_customConfigDir, "dll.d"));
        _scanningContext.Logger.LogDebug("Created temp SANE config dir {Dir}", _customConfigDir);
        return _customConfigDir;
    }

    internal OptionData SetOptions(ISaneDevice device, ScanOptions options)
    {
        var controller = new SaneOptionController(device, _scanningContext.Logger);
        var optionData = new OptionData
        {
            IsFeeder = options.PaperSource is PaperSource.Feeder or PaperSource.Duplex
        };

        if (options.PaperSource == PaperSource.Auto)
        {
            if (!controller.TrySet(SaneOptionNames.SOURCE, SaneOptionMatchers.Flatbed))
            {
                optionData.IsFeeder = controller.TrySet(SaneOptionNames.SOURCE, SaneOptionMatchers.Feeder);
            }
        }
        else if (options.PaperSource == PaperSource.Flatbed)
        {
            controller.TrySet(SaneOptionNames.SOURCE, SaneOptionMatchers.Flatbed);
        }
        else if (options.PaperSource == PaperSource.Feeder)
        {
            // We could throw NoFeederSupportException on failure, except this might be a feeder-only scanner.
            controller.TrySet(SaneOptionNames.SOURCE, SaneOptionMatchers.Feeder);
        }
        else if (options.PaperSource == PaperSource.Duplex)
        {
            if (!controller.TrySet(SaneOptionNames.SOURCE, SaneOptionMatchers.Duplex))
            {
                // If we can't set the source to Duplex, set it to Feeder instead.
                // We can then set AdfMode to Duplex.
                controller.TrySet(SaneOptionNames.SOURCE, SaneOptionMatchers.Feeder);
            }
            controller.TrySet(SaneOptionNames.ADF_MODE1, SaneOptionMatchers.Duplex);
            controller.TrySet(SaneOptionNames.ADF_MODE2, SaneOptionMatchers.Duplex);
        }

        var mode = options.BitDepth switch
        {
            BitDepth.BlackAndWhite => SaneOptionMatchers.BlackAndWhite,
            BitDepth.Grayscale => SaneOptionMatchers.Grayscale,
            _ => SaneOptionMatchers.Color
        };
        controller.TrySet(SaneOptionNames.MODE, mode, out optionData.Mode);

        SetResolution(options, controller, optionData);

        var scanAreaController = new SaneScanAreaController(controller);
        if (scanAreaController.CanSetArea)
        {
            var (minX, minY, maxX, maxY) = scanAreaController.GetBounds();
            var width = Math.Min((double) options.PageSize!.WidthInMm, maxX - minX);
            var height = Math.Min((double) options.PageSize.HeightInMm, maxY - minY);
            var deltaX = maxX - minX - width;
            var offsetX = options.PageAlign switch
            {
                HorizontalAlign.Left => deltaX,
                HorizontalAlign.Center => deltaX / 2,
                _ => 0
            };
            scanAreaController.SetArea(minX + offsetX, minY, minX + offsetX + width, minY + height);
        }

        foreach (var kvp in options.KeyValueOptions)
        {
            string name = kvp.Key;
            string value = kvp.Value;
            var opt = controller.GetOption(name);
            if (opt != null)
            {
                // TODO: Also implement bool value type
                if (opt.Type == SaneValueType.String)
                {
                    controller.TrySet(name, new SaneOptionMatcher([value]));
                }
                if (opt.Type is SaneValueType.Int or SaneValueType.Fixed && double.TryParse(value, out var doubleValue))
                {
                    controller.TrySet(name, doubleValue);
                }
            }
        }

        return optionData;
    }

    private void SetResolution(ScanOptions options, SaneOptionController controller, OptionData optionData)
    {
        var targetDpi = GetClosestResolution(options.Dpi, controller);

        if (controller.TrySet(SaneOptionNames.RESOLUTION, targetDpi))
        {
            if (controller.TryGet(SaneOptionNames.RESOLUTION, out var res))
            {
                optionData.XRes = res;
                optionData.YRes = res;
            }
        }
        else
        {
            controller.TrySet(SaneOptionNames.X_RESOLUTION, targetDpi);
            controller.TrySet(SaneOptionNames.Y_RESOLUTION, targetDpi);
            if (controller.TryGet(SaneOptionNames.X_RESOLUTION, out var xRes))
            {
                optionData.XRes = xRes;
            }
            if (controller.TryGet(SaneOptionNames.Y_RESOLUTION, out var yRes))
            {
                optionData.YRes = yRes;
            }
        }
        if (optionData.XRes <= 0) optionData.XRes = targetDpi;
        if (optionData.YRes <= 0) optionData.YRes = targetDpi;
    }

    private double GetClosestResolution(int dpi, SaneOptionController controller)
    {
        var targetDpi = (double) dpi;
        var opt = controller.GetOption(SaneOptionNames.RESOLUTION) ??
                  controller.GetOption(SaneOptionNames.X_RESOLUTION) ??
                  controller.GetOption(SaneOptionNames.Y_RESOLUTION);
        if (opt != null)
        {
            if (opt.ConstraintType == SaneConstraintType.Range)
            {
                targetDpi = targetDpi.Clamp(opt.Range!.Min, opt.Range.Max);
                if (opt.Range.Quant != 0)
                {
                    targetDpi -= (targetDpi - opt.Range.Min) % opt.Range.Quant;
                }
            }
            if (opt.ConstraintType == SaneConstraintType.WordList && opt.WordList!.Any())
            {
                targetDpi = opt.WordList!.OrderBy(x => Math.Abs(x - targetDpi)).First();
            }
        }
        if ((int) targetDpi != dpi)
        {
            _scanningContext.Logger.LogDebug("Correcting DPI from {InDpi} to {OutDpi}", dpi, targetDpi);
        }
        return targetDpi;
    }

    internal IMemoryImage? ScanPage(ISaneDevice device, IScanEvents scanEvents, OptionData optionData)
    {
        var data = ScanFrame(device, scanEvents, 0, out var p);
        if (data == null)
        {
            return null;
        }

        var page = p.Frame is SaneFrameType.Red or SaneFrameType.Green or SaneFrameType.Blue
            ? ProcessMultiFrameImage(device, scanEvents, p, data.GetBuffer())
            : ProcessSingleFrameImage(p, data.GetBuffer());
        page.SetResolution((float) optionData.XRes, (float) optionData.YRes);
        return page;
    }

    private IMemoryImage ProcessSingleFrameImage(SaneReadParameters p, byte[] data)
    {
        var (pixelFormat, subPixelType) = (depth: p.Depth, frame: p.Frame) switch
        {
            (1, SaneFrameType.Gray) => (ImagePixelFormat.BW1, SubPixelType.InvertedBit),
            (8, SaneFrameType.Gray) => (ImagePixelFormat.Gray8, SubPixelType.Gray),
            (8, SaneFrameType.Rgb) => (ImagePixelFormat.RGB24, SubPixelType.Rgb),
            _ => throw new InvalidOperationException(
                $"Unsupported transfer format: {p.Depth} bits per sample, {p.Frame} frame")
        };
        var image = _scanningContext.ImageContext.Create(p.PixelsPerLine, p.Lines, pixelFormat);
        var pixelInfo = new PixelInfo(p.PixelsPerLine, p.Lines, subPixelType, p.BytesPerLine);
        new CopyBitwiseImageOp().Perform(data, pixelInfo, image);
        return image;
    }

    private IMemoryImage ProcessMultiFrameImage(ISaneDevice device, IScanEvents scanEvents, SaneReadParameters p,
        byte[] data)
    {
        var image = _scanningContext.ImageContext.Create(p.PixelsPerLine, p.Lines, ImagePixelFormat.RGB24);
        var pixelInfo = new PixelInfo(p.PixelsPerLine, p.Lines, SubPixelType.Gray, p.BytesPerLine);

        // Use the first buffer, then read two more buffers and use them so we get all 3 channels
        new CopyBitwiseImageOp { DestChannel = ToChannel(p.Frame) }.Perform(data, pixelInfo, image);
        ReadSingleChannelFrame(device, scanEvents, 1, pixelInfo, image);
        ReadSingleChannelFrame(device, scanEvents, 2, pixelInfo, image);
        return image;
    }

    private void ReadSingleChannelFrame(ISaneDevice device, IScanEvents scanEvents, int frame, PixelInfo pixelInfo,
        IMemoryImage image)
    {
        var data = ScanFrame(device, scanEvents, frame, out var p)
                   ?? throw new DeviceException("SANE unexpected last frame");
        new CopyBitwiseImageOp { DestChannel = ToChannel(p.Frame) }.Perform(data.GetBuffer(), pixelInfo, image);
    }

    private ColorChannel ToChannel(SaneFrameType frame) => frame switch
    {
        SaneFrameType.Red => ColorChannel.Red,
        SaneFrameType.Green => ColorChannel.Green,
        SaneFrameType.Blue => ColorChannel.Blue,
        _ => throw new ArgumentException()
    };

    internal MemoryStream? ScanFrame(ISaneDevice device, IScanEvents scanEvents, int frame, out SaneReadParameters p)
    {
        device.Start();
        if (frame == 0)
        {
            scanEvents.PageStart();
        }

        p = device.GetParameters();
        bool isMultiFrame = p.Frame is SaneFrameType.Red or SaneFrameType.Green or SaneFrameType.Blue;
        // p.Lines can be -1, in which case we don't know the frame size ahead of time
        var frameSize = p.Lines == -1 ? 0 : p.BytesPerLine * p.Lines;
        var currentProgress = frame * frameSize;
        var totalProgress = isMultiFrame ? frameSize * 3 : frameSize;
        var buffer = new byte[65536];
        if (totalProgress > 0)
        {
            scanEvents.PageProgress(currentProgress / (double) totalProgress);
        }

        var dataStream = new MemoryStream(frameSize);
        while (device.Read(buffer, out var len))
        {
            dataStream.Write(buffer, 0, len);
            currentProgress += len;
            if (totalProgress > 0)
            {
                scanEvents.PageProgress(currentProgress / (double) totalProgress);
            }
        }

        if (dataStream.Length == 0)
        {
            return null;
        }
        // Now that we've read the data we know the exact frame size and can work backwards to get the number of lines.
        p.Lines = (int) dataStream.Length / p.BytesPerLine;

        return dataStream;
    }

    internal class OptionData
    {
        public bool IsFeeder;
        public double XRes;
        public double YRes;
        public string? Mode;
    }

    //     if (options.BitDepth == BitDepth.Color)
    //     {
    //         ChooseStringOption("--mode", x => x == "Color");
    //         ChooseNumericOption("--depth", 8);
    //     }
    //     else if (options.BitDepth == BitDepth.Grayscale)
    //     {
    //         ChooseStringOption("--mode", x => x == "Gray");
    //         ChooseNumericOption("--depth", 8);
    //     }
    //     else if (options.BitDepth == BitDepth.BlackAndWhite)
    //     {
    //         if (!ChooseStringOption("--mode", x => x == "Lineart"))
    //         {
    //             ChooseStringOption("--mode", x => x == "Halftone");
    //         }
    //         ChooseNumericOption("--depth", 1);
    //         ChooseNumericOption("--threshold", (-options.Brightness + 1000) / 20m);
    //     }
}