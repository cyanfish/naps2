using System.Threading;
using Microsoft.Extensions.Logging;
using NAPS2.Escl;
using NAPS2.Escl.Client;
using NAPS2.Pdf;
using NAPS2.Scan.Exceptions;

namespace NAPS2.Scan.Internal.Escl;

internal class EsclScanDriver : IScanDriver
{
    private readonly ScanningContext _scanningContext;

    public EsclScanDriver(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public async Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback)
    {
        // TODO: Run location in a persistent background service
        using var locator = new EsclServiceLocator(service =>
        {
            // Store both the IP and UUID so we can preferentially find by the IP, but also fall back to looking for
            // the UUID in case the IP changed
            var ip = service.IpV4 ?? service.IpV6;
            var id = $"{ip}|{service.Uuid}";
            var name = string.IsNullOrEmpty(service.ScannerName)
                ? $"{ip}"
                : $"{service.ScannerName} ({ip})";
            callback(new ScanDevice(id, name));
        });
        locator.Logger = _scanningContext.Logger;
        locator.Start();
        try
        {
            await Task.Delay(2000, cancelToken);
        }
        catch (TaskCanceledException)
        {
        }
    }

    public async Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<IMemoryImage> callback)
    {
        if (cancelToken.IsCancellationRequested) return;

        var service = await FindDeviceEsclService(options) ?? throw new DeviceException(SdkResources.DeviceOffline);

        if (cancelToken.IsCancellationRequested) return;

        var client = new EsclClient(service)
        {
            Logger = _scanningContext.Logger
        };

        var caps = await client.GetCapabilities();
        var status = await client.GetStatus();
        var scanSettings = GetScanSettings(options, caps);

        if (cancelToken.IsCancellationRequested) return;

        VerifyStatus(status, scanSettings);

        var job = await client.CreateScanJob(scanSettings);

        var cancelOnce = new Once(() => client.CancelJob(job).AssertNoAwait());
        using var cancelReg = cancelToken.Register(cancelOnce.Run);

        try
        {
            while (true)
            {
                // TODO: Can we do progress reporting?
                scanEvents.PageStart();
                RawDocument? doc;
                try
                {
                    doc = await client.NextDocument(job);
                }
                catch (Exception ex)
                {
                    _scanningContext.Logger.LogDebug(ex, "ESCL error");
                    break;
                }
                if (doc == null) break;
                foreach (var image in GetImagesFromRawDocument(options, doc))
                {
                    callback(image);
                }
            }
        }
        finally
        {
            cancelOnce.Run();
        }
    }

    private static void VerifyStatus(EsclScannerStatus status, EsclScanSettings scanSettings)
    {
        if (status.State is EsclScannerState.Processing or EsclScannerState.Testing or EsclScannerState.Stopped)
        {
            throw new DeviceException(SdkResources.DeviceBusy);
        }
        if (status.State == EsclScannerState.Down)
        {
            throw new DeviceException(SdkResources.DeviceOffline);
        }
        if (scanSettings.InputSource == EsclInputSource.Feeder)
        {
            if (status.AdfState == EsclAdfState.ScannerAdfEmpty)
            {
                throw new DeviceException(SdkResources.NoPagesInFeeder);
            }
            if (status.AdfState == EsclAdfState.ScannerAdfJam)
            {
                throw new DeviceException(SdkResources.DevicePaperJam);
            }
            if (status.AdfState is not (EsclAdfState.Unknown or EsclAdfState.ScannerAdfProcessing
                or EsclAdfState.ScannedAdfLoaded))
            {
                throw new DeviceException(status.AdfState.ToString());
            }
        }
    }

    private async Task<EsclService?> FindDeviceEsclService(ScanOptions options)
    {
        var foundTcs = new TaskCompletionSource<EsclService?>();
        using var locator = new EsclServiceLocator(service =>
        {
            var parts = options.Device!.ID.Split('|');
            var ip = parts[0];
            var uuid = parts[1];
            if ((service.IpV4 ?? service.IpV6!).ToString() == ip || service.Uuid == uuid)
            {
                foundTcs.TrySetResult(service);
            }
        });
        Task.Delay(2000).ContinueWith(_ => foundTcs.TrySetResult(null)).AssertNoAwait();
        locator.Logger = _scanningContext.Logger;
        locator.Start();
        return await foundTcs.Task;
    }

    private IEnumerable<IMemoryImage> GetImagesFromRawDocument(ScanOptions options, RawDocument doc)
    {
        if (doc.ContentType == "application/pdf")
        {
            // TODO: For SDK some kind an error message if Pdfium isn't present
            var renderer = new PdfiumPdfRenderer();
            foreach (var image in renderer.Render(_scanningContext.ImageContext, doc.Data, doc.Data.Length,
                         PdfRenderSize.FromDpi(options.Dpi)))
            {
                yield return image;
            }
        }
        else
        {
            yield return _scanningContext.ImageContext.Load(doc.Data);
        }
    }

    private EsclScanSettings GetScanSettings(ScanOptions options, EsclCapabilities caps)
    {
        if (options.PaperSource == PaperSource.Feeder && caps.AdfSimplexCaps == null)
        {
            throw new NoFeederSupportException();
        }
        if (options.PaperSource == PaperSource.Duplex && caps.AdfDuplexCaps == null)
        {
            throw new NoDuplexSupportException();
        }
        if (options.PaperSource is PaperSource.Flatbed or PaperSource.Auto
            && caps.PlatenCaps == null && caps.AdfSimplexCaps != null)
        {
            options.PaperSource = PaperSource.Feeder;
        }

        var (inputCaps, inputSource, duplex) = options.PaperSource switch
        {
            PaperSource.Feeder => (caps.AdfSimplexCaps, EsclInputSource.Feeder, false),
            PaperSource.Duplex => (caps.AdfDuplexCaps, EsclInputSource.Feeder, true),
            _ => (caps.PlatenCaps, EsclInputSource.Platen, false),
        };
        inputCaps ??= new EsclInputCaps();

        var colorMode = options.BitDepth switch
        {
            BitDepth.Color => EsclColorMode.RGB24,
            BitDepth.Grayscale => EsclColorMode.Grayscale8,
            BitDepth.BlackAndWhite => EsclColorMode.BlackAndWhite1,
            _ => EsclColorMode.RGB24
        };
        int dpi = options.Dpi;

        var settingProfile = inputCaps.SettingProfiles.FirstOrDefault(x => x.ColorModes.Contains(colorMode))
                             ?? inputCaps.SettingProfiles.FirstOrDefault();

        if (settingProfile != null)
        {
            colorMode = MaybeCorrectColorMode(settingProfile, colorMode);

            var discreteResolutions =
                settingProfile.DiscreteResolutions.Where(res => res.XResolution == res.YResolution)
                    .Select(res => res.XResolution).ToList();
            if (discreteResolutions.Any())
            {
                dpi = discreteResolutions.OrderBy(v => Math.Abs(v - dpi)).First();
            }

            if (settingProfile.XResolutionRange != null && settingProfile.YResolutionRange != null)
            {
                int min = Math.Max(settingProfile.XResolutionRange.Min, settingProfile.YResolutionRange.Min);
                int max = Math.Min(settingProfile.XResolutionRange.Max, settingProfile.YResolutionRange.Max);
                dpi = dpi.Clamp(min, max);
            }
        }

        return new EsclScanSettings
        {
            Width = (int) Math.Round(options.PageSize!.WidthInInches * 300),
            Height = (int) Math.Round(options.PageSize!.HeightInInches * 300),
            XResolution = dpi,
            YResolution = dpi,
            ColorMode = colorMode,
            InputSource = inputSource,
            Duplex = duplex,
            DocumentFormat = options.BitDepth == BitDepth.BlackAndWhite || options.MaxQuality
                ? "application/pdf" // TODO: Use PNG if available?
                : "image/jpeg"
            // TODO: Offset, brightness/contrast, quality, etc.
        };
    }

    private static EsclColorMode MaybeCorrectColorMode(EsclSettingProfile settingProfile, EsclColorMode colorMode)
    {
        if (settingProfile.ColorModes.Contains(colorMode))
        {
            return colorMode;
        }
        if (colorMode == EsclColorMode.BlackAndWhite1)
        {
            if (settingProfile.ColorModes.Contains(EsclColorMode.Grayscale8))
            {
                colorMode = EsclColorMode.Grayscale8;
            }
            else if (settingProfile.ColorModes.Contains(EsclColorMode.RGB24))
            {
                colorMode = EsclColorMode.RGB24;
            }
        }
        else if (colorMode == EsclColorMode.Grayscale8 && settingProfile.ColorModes.Contains(EsclColorMode.RGB24))
        {
            colorMode = EsclColorMode.RGB24;
        }
        return colorMode;
    }
}