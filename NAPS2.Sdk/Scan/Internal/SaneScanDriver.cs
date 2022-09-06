using System.Runtime.InteropServices;
using System.Threading;
using NAPS2.Images.Bitwise;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Sane;
using static NAPS2.Scan.Sane.SaneNativeLibrary;

namespace NAPS2.Scan.Internal;

internal class SaneScanDriver : IScanDriver
{
    private static readonly Dictionary<string, SaneOptionCollection> SaneOptionCache = new();

    private readonly ScanningContext _scanningContext;

    public SaneScanDriver(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    private SaneNativeLibrary Native => Instance;

    public Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        return Task.Run(() =>
        {
            var deviceList = new List<ScanDevice>();

            // TODO: Run SANE in a worker process so we can parallelize
            lock (Native)
            {
                // The doc says we only need to init once, but in practice it seems like sane_get_devices
                // caches its values unless we exit and re-init.
                Native.sane_init(out _, IntPtr.Zero);
                try
                {
                    HandleStatus(Native.sane_get_devices(out var deviceListPtr, 0));
                    IntPtr devicePtr;
                    int offset = 0;
                    while ((devicePtr = Marshal.ReadIntPtr(deviceListPtr, offset++ * IntPtr.Size)) != IntPtr.Zero)
                    {
                        var device = Marshal.PtrToStructure<SANE_Device>(devicePtr);
                        // TODO: We can use .type and .vendor to help pick an icon etc.
                        // https://sane-project.gitlab.io/standard/api.html#device-descriptor-type
                        deviceList.Add(new ScanDevice
                        {
                            ID = device.name,
                            Name = device.model
                        });
                    }
                }
                finally
                {
                    Native.sane_exit();
                }
            }

            return deviceList;
        });
    }

    private void HandleStatus(SANE_Status status)
    {
        switch (status)
        {
            case SANE_Status.Good:
                return;
            case SANE_Status.Cancelled:
                throw new OperationCanceledException();
            case SANE_Status.NoDocs:
                throw new NoPagesException();
            case SANE_Status.DeviceBusy:
                throw new DeviceException(SdkResources.DeviceBusy);
            case SANE_Status.Invalid:
                // TODO: Maybe not always correct? e.g. when setting options
                throw new DeviceException(SdkResources.DeviceOffline);
            case SANE_Status.Jammed:
                throw new DeviceException(SdkResources.DevicePaperJam);
            case SANE_Status.CoverOpen:
                throw new DeviceException(SdkResources.DeviceCoverOpen);
            default:
                throw new DeviceException($"SANE error: {status}");
        }
    }

    public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<IMemoryImage> callback)
    {
        return Task.Run(() =>
        {
            lock (Native)
            {
                Native.sane_init(out _, IntPtr.Zero);
                try
                {
                    if (cancelToken.IsCancellationRequested) return;
                    HandleStatus(Native.sane_open(options.Device!.ID!, out var handle));
                    try
                    {
                        if (cancelToken.IsCancellationRequested) return;
                        // TODO: Set up options
                        cancelToken.Register(() => Native.sane_cancel(handle));

                        // TODO: Can we validate whether it's really an adf?
                        if (options.PaperSource == PaperSource.Flatbed)
                        {
                            var image = ScanPage(handle, scanEvents) ??
                                        throw new DeviceException("SANE expected image");
                            callback(image);
                        }
                        else
                        {
                            while (ScanPage(handle, scanEvents) is { } image)
                            {
                                callback(image);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    finally
                    {
                        Native.sane_close(handle);
                    }
                }
                finally
                {
                    Native.sane_exit();
                }
            }

            // // TODO: Test ADF
            // var keyValueOptions = new Lazy<KeyValueScanOptions>(() => GetKeyValueOptions(options));
            // scanEvents.PageStart();
            // bool result = Transfer(keyValueOptions, options, cancelToken, scanEvents, callback);
            //
            // if (result && options.PaperSource != PaperSource.Flatbed)
            // {
            //     try
            //     {
            //         do
            //         {
            //             scanEvents.PageStart();
            //         } while (Transfer(keyValueOptions, options, cancelToken, scanEvents, callback));
            //     }
            //     catch (Exception e)
            //     {
            //         Log.ErrorException("Error in SANE. This may be a normal ADF termination.", e);
            //     }
            // }
        });
    }

    private IMemoryImage? ScanPage(IntPtr handle, IScanEvents scanEvents)
    {
        // TODO: Fix up events
        var data = ScanFrame(handle, scanEvents, 0, out var p);
        if (data == null)
        {
            return null;
        }
        if (p.frame is SANE_Frame.Red or SANE_Frame.Green or SANE_Frame.Blue)
        {
            return ProcessMultiFrameImage(handle, scanEvents, p, data);
        }
        return ProcessSingleFrameImage(p, data);
    }

    private IMemoryImage ProcessSingleFrameImage(SANE_Parameters p, byte[] data)
    {
        var (pixelFormat, subPixelType) = (p.depth, p.frame) switch
        {
            (1, SANE_Frame.Gray) => (ImagePixelFormat.BW1, SubPixelType.Bit),
            (8, SANE_Frame.Gray) => (ImagePixelFormat.Gray8, SubPixelType.Gray),
            (8, SANE_Frame.Rgb) => (ImagePixelFormat.RGB24, SubPixelType.Rgb),
            _ => throw new InvalidOperationException(
                $"Unsupported transfer format: {p.depth} bits per sample, {p.frame} frame")
        };
        var image = _scanningContext.ImageContext.Create(p.pixels_per_line, p.lines, pixelFormat);
        var pixelInfo = new PixelInfo(p.pixels_per_line, p.lines, subPixelType);
        new CopyBitwiseImageOp().Perform(data, pixelInfo, image);
        return image;
    }

    private IMemoryImage ProcessMultiFrameImage(IntPtr handle, IScanEvents scanEvents, SANE_Parameters p, byte[] data)
    {
        var image = _scanningContext.ImageContext.Create(p.pixels_per_line, p.lines, ImagePixelFormat.RGB24);
        var pixelInfo = new PixelInfo(p.pixels_per_line, p.lines, SubPixelType.Gray);

        // Use the first buffer, then read two more buffers and use them so we get all 3 channels
        new CopyBitwiseImageOp { DestChannel = ToChannel(p.frame) }.Perform(data, pixelInfo, image);
        ReadSingleChannelFrame(handle, scanEvents, 1, pixelInfo, image);
        ReadSingleChannelFrame(handle, scanEvents, 2, pixelInfo, image);
        return image;
    }

    private void ReadSingleChannelFrame(IntPtr handle, IScanEvents scanEvents, int frame, PixelInfo pixelInfo,
        IMemoryImage image)
    {
        var data = ScanFrame(handle, scanEvents, frame, out var p)
                   ?? throw new DeviceException("SANE unexpected last frame");
        new CopyBitwiseImageOp { DestChannel = ToChannel(p.frame) }.Perform(data, pixelInfo, image);
    }

    private ColorChannel ToChannel(SANE_Frame frame) => frame switch
    {
        SANE_Frame.Red => ColorChannel.Red,
        SANE_Frame.Green => ColorChannel.Green,
        SANE_Frame.Blue => ColorChannel.Blue,
        _ => throw new ArgumentException()
    };

    private byte[]? ScanFrame(IntPtr handle, IScanEvents scanEvents, int frame, out SANE_Parameters p)
    {
        HandleStatus(Native.sane_start(handle));
        if (frame == 0)
        {
            scanEvents.PageStart();
        }
        HandleStatus(Native.sane_get_parameters(handle, out p));
        bool isMultiFrame = p.frame is SANE_Frame.Red or SANE_Frame.Green or SANE_Frame.Blue;
        var frameSize = p.bytes_per_line * p.lines;
        var currentProgress = frame * frameSize;
        var totalProgress = isMultiFrame ? frameSize * 3 : frameSize;
        var data = new byte[frameSize];
        var index = 0;
        var buffer = new byte[65536];
        scanEvents.PageProgress(currentProgress / (double) totalProgress);

        SANE_Status status;
        while ((status = Native.sane_read(handle, buffer, buffer.Length, out var len)) == SANE_Status.Good)
        {
            Array.Copy(buffer, 0, data, index, len);
            index += len;
            currentProgress += len;
            scanEvents.PageProgress(currentProgress / (double) totalProgress);
        }

        if (status == SANE_Status.NoDocs)
        {
            return null;
        }
        if (status != SANE_Status.Eof)
        {
            HandleStatus(status);
        }
        if (index != frameSize)
        {
            throw new DeviceException($"SANE unexpected data length, got {index}, expected {frameSize}");
        }
        return data;
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