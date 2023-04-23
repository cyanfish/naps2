using System.Threading;
using NAPS2.Images.Bitwise;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Internal.Sane.Native;

namespace NAPS2.Scan.Internal.Sane;

internal class SaneScanDriver : IScanDriver
{
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
        // Special cases for sane-escl and sane-airscan.
        if (backend == "escl")
        {
            // We include the vendor as it's excluded from the model, and we include the full name instead of
            // just the backend as that has the IP address.
            return new ScanDevice(device.Name, $"{device.Vendor} {device.Model} ({device.Name})");
        }
        if (backend == "airscan")
        {
            // We include the device type which has the IP address.
            // TODO: The sane-airscan ID isn't persistent, can we work around that somehow and persist based on IP?
            return new ScanDevice(device.Name, $"{device.Model} ({backend}:{device.Type})");
        }
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
                SetOptions(device, options, out bool isFeeder);
                // TODO: We apparently need to cancel even upon normal completion, i.e. one sane_cancel per sane_start
                cancelToken.Register(device.Cancel);

                if (!isFeeder)
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

    internal void SetOptions(ISaneDevice device, ScanOptions options, out bool isFeeder)
    {
        var controller = new SaneOptionController(device, _scanningContext.Logger);

        isFeeder = options.PaperSource is PaperSource.Feeder or PaperSource.Duplex; 
        if (options.PaperSource == PaperSource.Auto)
        {
            if (!controller.TrySet(SaneOptionNames.SOURCE, SaneOptionMatchers.Flatbed))
            {
                isFeeder = controller.TrySet(SaneOptionNames.SOURCE, SaneOptionMatchers.Feeder);
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
            controller.TrySet(SaneOptionNames.SOURCE, SaneOptionMatchers.Duplex);
            controller.TrySet(SaneOptionNames.ADF_MODE1, SaneOptionMatchers.Duplex);
            controller.TrySet(SaneOptionNames.ADF_MODE2, SaneOptionMatchers.Duplex);
        }

        var mode = options.BitDepth switch
        {
            BitDepth.BlackAndWhite => SaneOptionMatchers.BlackAndWhite,
            BitDepth.Grayscale => SaneOptionMatchers.Grayscale,
            _ => SaneOptionMatchers.Color
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
}