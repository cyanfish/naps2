using System.Threading;
using NAPS2.Images.Bitwise;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Internal.Sane.Native;

namespace NAPS2.Scan.Internal.Sane;

internal class SaneScanDriver : IScanDriver
{
    private static readonly HashSet<string> FlatbedStrs = new[]
    {
        SaneOptionTranslations.Flatbed,
        SaneOptionTranslations.FB,
        SaneOptionTranslations.fb
    }.SelectMany(x => x).ToHashSet();

    private static readonly HashSet<string> FeederStrs = new[]
    {
        SaneOptionTranslations.ADF,
        SaneOptionTranslations.adf,
        SaneOptionTranslations.Automatic_Document_Feeder,
        SaneOptionTranslations.ADF_Front
    }.SelectMany(x => x).ToHashSet();

    private static readonly HashSet<string> DuplexStrs = new[]
    {
        SaneOptionTranslations.Duplex,
        SaneOptionTranslations.ADF_Duplex
    }.SelectMany(x => x).ToHashSet();

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
        return Task.Run(() =>
        {
            // TODO: This is crashing after a delay for no apparent reason.
            // That's okay because we're in a worker process, but ideally we could fix it in SANE.
            using var client = new SaneClient(Installation);
            // TODO: We can use device.type and .vendor to help pick an icon etc.
            // https://sane-project.gitlab.io/standard/api.html#device-descriptor-type
            if (Installation.CanStreamDevices)
            {
                client.StreamDevices(device => callback(GetScanDevice(device)), cancelToken);
            }
            else
            {
                foreach (var device in client.GetDevices())
                {
                    callback(GetScanDevice(device));
                }
            }
        });
    }

    private static ScanDevice GetScanDevice(SaneDeviceInfo device)
    {
        var backend = device.Name.Split(':')[0];
        return new ScanDevice(device.Name, $"{device.Model} ({backend})");
    }

    public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<IMemoryImage> callback)
    {
        return Task.Run(() =>
        {
            bool hasAtLeastOneImage = false;
            try
            {
                using var client = new SaneClient(Installation);
                if (cancelToken.IsCancellationRequested) return;
                using var device = client.OpenDevice(options.Device!.ID!);
                if (cancelToken.IsCancellationRequested) return;
                SetOptions(device, options);
                // TODO: We apparently need to cancel even upon normal completion, i.e. one sane_cancel per sane_start
                cancelToken.Register(device.Cancel);

                // TODO: Can we validate flatbed and feeder support?
                if (options.PaperSource is PaperSource.Flatbed or PaperSource.Auto)
                {
                    var image = ScanPage(device, scanEvents) ??
                                throw new DeviceException("SANE expected image");
                    callback(image);
                }
                else
                {
                    while (ScanPage(device, scanEvents) is { } image)
                    {
                        hasAtLeastOneImage = true;
                        callback(image);
                    }
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
                            throw new NoPagesException();
                        }

                        break;
                    case SaneStatus.DeviceBusy:
                        throw new DeviceException(SdkResources.DeviceBusy);
                    case SaneStatus.Invalid:
                        // TODO: Maybe not always correct? e.g. when setting options
                        throw new DeviceException(SdkResources.DeviceOffline);
                    case SaneStatus.Jammed:
                        throw new DeviceException(SdkResources.DevicePaperJam);
                    case SaneStatus.CoverOpen:
                        throw new DeviceException(SdkResources.DeviceCoverOpen);
                    default:
                        throw new DeviceException($"SANE error: {ex.Status}");
                }
            }
        });
    }

    private void SetOptions(SaneDevice device, ScanOptions options)
    {
        var controller = new SaneOptionController(device, _scanningContext.Logger);

        // TODO: Check the SOURCE option possible values to check flatbed/feeder support
        if (options.PaperSource is PaperSource.Flatbed or PaperSource.Auto)
        {
            controller.TrySet(SaneOptionNames.SOURCE, FlatbedStrs);
        }
        else if (options.PaperSource == PaperSource.Feeder)
        {
            // We could throw NoFeederSupportException on failure, except this might be a feeder-only scanner.
            controller.TrySet(SaneOptionNames.SOURCE, FeederStrs);
        }
        else if (options.PaperSource == PaperSource.Duplex)
        {
            controller.TrySet(SaneOptionNames.SOURCE, DuplexStrs);
            controller.TrySet(SaneOptionNames.ADF_MODE1, DuplexStrs);
            controller.TrySet(SaneOptionNames.ADF_MODE2, DuplexStrs);
        }

        var mode = options.BitDepth switch
        {
            BitDepth.BlackAndWhite => SaneOptionTranslations.Lineart,
            BitDepth.Grayscale => SaneOptionTranslations.Gray,
            _ => SaneOptionTranslations.Color
        };
        controller.TrySet(SaneOptionNames.MODE, mode);

        // TODO: Get closest resolution value
        if (!controller.TrySet(SaneOptionNames.RESOLUTION, options.Dpi))
        {
            controller.TrySet(SaneOptionNames.X_RESOLUTION, options.Dpi);
            controller.TrySet(SaneOptionNames.Y_RESOLUTION, options.Dpi);
        }

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
    }

    internal IMemoryImage? ScanPage(ISaneDevice device, IScanEvents scanEvents)
    {
        var data = ScanFrame(device, scanEvents, 0, out var p);
        if (data == null)
        {
            return null;
        }

        if (p.Frame is SaneFrameType.Red or SaneFrameType.Green or SaneFrameType.Blue)
        {
            return ProcessMultiFrameImage(device, scanEvents, p, data.GetBuffer());
        }

        return ProcessSingleFrameImage(p, data.GetBuffer());
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

    // private KeyValueScanOptions GetKeyValueOptions(ScanOptions options)
    // {
    //     var availableOptions =
    //         SaneOptionCache.GetOrSet(options.Device!.ID!, () => GetAvailableOptions(options.Device!.ID!));
    //     var keyValueOptions = new KeyValueScanOptions(options.SaneOptions.KeyValueOptions ?? new KeyValueScanOptions());
    //
    //     bool ChooseStringOption(string name, Func<string, bool> match)
    //     {
    //         var opt = availableOptions.Get(name);
    //         var choice = opt?.StringList?.FirstOrDefault(match);
    //         if (choice != null)
    //         {
    //             keyValueOptions[name] = choice;
    //             return true;
    //         }
    //         return false;
    //     }
    //
    //     bool ChooseNumericOption(string name, decimal value)
    //     {
    //         var opt = availableOptions.Get(name);
    //         if (opt?.ConstraintType == SaneConstraintType.WordList)
    //         {
    //             var choice = opt.WordList?.OrderBy(x => Math.Abs(x - value)).FirstOrDefault();
    //             if (choice != null)
    //             {
    //                 keyValueOptions[name] = choice.Value.ToString(CultureInfo.InvariantCulture);
    //                 return true;
    //             }
    //         }
    //         else if (opt?.ConstraintType == SaneConstraintType.Range)
    //         {
    //             if (value < opt.Range!.Min)
    //             {
    //                 value = opt.Range.Min;
    //             }
    //             if (value > opt.Range.Max)
    //             {
    //                 value = opt.Range.Max;
    //             }
    //             if (opt.Range.Quant != 0)
    //             {
    //                 var mod = (value - opt.Range.Min) % opt.Range.Quant;
    //                 if (mod != 0)
    //                 {
    //                     value = mod < opt.Range.Quant / 2 ? value - mod : value + opt.Range.Quant - mod;
    //                 }
    //             }
    //             keyValueOptions[name] = value.ToString("0.#####", CultureInfo.InvariantCulture);
    //             return true;
    //         }
    //         return false;
    //     }
    //
    //     bool IsFlatbedChoice(string choice) =>
    //         choice.IndexOf("flatbed", StringComparison.InvariantCultureIgnoreCase) >= 0;
    //
    //     bool IsFeederChoice(string choice) => new[] { "adf", "feeder", "simplex" }.Any(x =>
    //         choice.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) >= 0);
    //
    //     bool IsDuplexChoice(string choice) =>
    //         choice.IndexOf("duplex", StringComparison.InvariantCultureIgnoreCase) >= 0;
    //
    //     if (options.PaperSource == PaperSource.Flatbed)
    //     {
    //         ChooseStringOption("--source", IsFlatbedChoice);
    //     }
    //     else if (options.PaperSource == PaperSource.Feeder)
    //     {
    //         if (!ChooseStringOption("--source", x => IsFeederChoice(x) && !IsDuplexChoice(x)) &&
    //             !ChooseStringOption("--source", IsFeederChoice) &&
    //             !ChooseStringOption("--source", IsDuplexChoice))
    //         {
    //             throw new NoFeederSupportException();
    //         }
    //     }
    //     else if (options.PaperSource == PaperSource.Duplex)
    //     {
    //         if (!ChooseStringOption("--source", IsDuplexChoice))
    //         {
    //             throw new NoDuplexSupportException();
    //         }
    //     }
    //
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
    //
    //     var width = options.PageSize!.WidthInMm;
    //     var height = options.PageSize.HeightInMm;
    //     ChooseNumericOption("-x", width);
    //     ChooseNumericOption("-y", height);
    //     var maxWidth = availableOptions.Get("-l")?.Range?.Max;
    //     var maxHeight = availableOptions.Get("-t")?.Range?.Max;
    //     if (maxWidth != null)
    //     {
    //         if (options.PageAlign == HorizontalAlign.Center)
    //         {
    //             ChooseNumericOption("-l", (maxWidth.Value - width) / 2);
    //         }
    //         else if (options.PageAlign == HorizontalAlign.Right)
    //         {
    //             ChooseNumericOption("-l", maxWidth.Value - width);
    //         }
    //         else
    //         {
    //             ChooseNumericOption("-l", 0);
    //         }
    //     }
    //     if (maxHeight != null)
    //     {
    //         ChooseNumericOption("-t", 0);
    //     }
    //
    //     if (!ChooseNumericOption("--resolution", options.Dpi))
    //     {
    //         ChooseNumericOption("--x-resolution", options.Dpi);
    //         ChooseNumericOption("--y-resolution", options.Dpi);
    //     }
    //
    //     return keyValueOptions;
    // }
}