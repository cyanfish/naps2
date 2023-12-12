using System.Threading;
using Microsoft.Extensions.Logging;
using NAPS2.Escl;
using NAPS2.Escl.Client;
using NAPS2.Pdf;
using NAPS2.Remoting;
using NAPS2.Scan.Exceptions;
using NAPS2.Serialization;

namespace NAPS2.Scan.Internal.Escl;

internal class EsclScanDriver : IScanDriver
{
    private const int MAX_DOCUMENT_TRIES = 5;
    private const int DOCUMENT_RETRY_INTERVAL = 1000;

    private readonly ScanningContext _scanningContext;
    private readonly ILogger _logger;

    public static string GetUuid(ScanDevice device)
    {
        var parts = device.ID.Split('|');
        if (parts.Length != 2)
        {
            throw new ArgumentException("Invalid ESCL device ID");
        }
        return parts[1];
    }

    public EsclScanDriver(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
        _logger = scanningContext.Logger;
    }

    public async Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback)
    {
        // TODO: Run location in a persistent background service
        var localIPsTask = options.ExcludeLocalIPs ? LocalIPsHelper.Get() : null;
        using var locator = new EsclServiceLocator(service =>
        {
            // Store both the IP and UUID so we can preferentially find by the IP, but also fall back to looking for
            // the UUID in case the IP changed
            var ip = service.IpV4 ?? service.IpV6!;
            if (options.ExcludeLocalIPs && localIPsTask!.Result.Contains(ip.ToString()))
            {
                return;
            }
            var id = $"{ip}|{service.Uuid}";
            var name = string.IsNullOrEmpty(service.ScannerName)
                ? $"{ip}"
                : $"{service.ScannerName} ({ip})";
            callback(new ScanDevice(Driver.Escl, id, name));
        });
        locator.Logger = _logger;
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
            Logger = _logger,
            CancelToken = cancelToken
        };

        try
        {
            var caps = await client.GetCapabilities();
            var status = await client.GetStatus();
            var scanSettings = GetScanSettings(options, caps);
            bool hasProgressExtension = caps.Naps2Extensions?.Contains("Progress") ?? false;
            bool hasErrorDetailsExtension = caps.Naps2Extensions?.Contains("ErrorDetails") ?? false;
            Action<double>? progressCallback = hasProgressExtension ? scanEvents.PageProgress : null;

            if (cancelToken.IsCancellationRequested) return;

            VerifyStatus(status, scanSettings);

            var job = await client.CreateScanJob(scanSettings);

            var cancelOnce = new Once(() => client.CancelJob(job).AssertNoAwait());
            using var cancelReg = cancelToken.Register(cancelOnce.Run);

            try
            {
                if (scanSettings.InputSource == EsclInputSource.Platen)
                {
                    scanEvents.PageStart();
                }
                while (true)
                {
                    if (scanSettings.InputSource != EsclInputSource.Platen)
                    {
                        scanEvents.PageStart();
                    }
                    var doc = await GetNextDocumentWithRetries(client, job, progressCallback);
                    if (doc == null) break;
                    foreach (var image in GetImagesFromRawDocument(options, doc))
                    {
                        callback(image);
                    }
                }
            }
            catch (Exception)
            {
                cancelOnce.Run();
                // The root cause for the exception might be a server-side scanning error, so prefer to throw a more
                // descriptive error rather than an HTTP-based exception.
                if (hasErrorDetailsExtension)
                {
                    await CheckErrorDetails(client, job);
                }
                // If not, then just throw the error we got
                throw;
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async Task<RawDocument?> GetNextDocumentWithRetries(
        EsclClient client, EsclJob job, Action<double>? progress)
    {
        int retries = 0;
        while (true)
        {
            try
            {
                return await client.NextDocument(job, progress);
            }
            catch (Exception)
            {
                if (++retries > MAX_DOCUMENT_TRIES)
                {
                    _logger.LogDebug("ESCL NextDocument failed, no more retries left");
                    throw;
                }
                var status = await client.GetStatus();
                var jobState = status.JobStates.Get(job.UriPath);
                if (jobState is not (EsclJobState.Pending or EsclJobState.Processing or EsclJobState.Unknown))
                {
                    // Only retry if the job is pending or processing
                    _logger.LogDebug("ESCL NextDocument failed, not retrying as job state is {State}", jobState);
                    throw;
                }
                _logger.LogDebug("ESCL NextDocument failed, retrying as job state is {State}", jobState);
                await Task.Delay(DOCUMENT_RETRY_INTERVAL);
            }
        }
    }

    private async Task CheckErrorDetails(EsclClient client, EsclJob job)
    {
        string? errorDetails = null;
        try
        {
            errorDetails = await client.ErrorDetails(job);
        }
        catch (Exception)
        {
            // Ignore
        }
        if (!string.IsNullOrEmpty(errorDetails))
        {
            RemotingHelper.HandleErrors(errorDetails!.FromXml<Error>());
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
        var deviceUuid = GetUuid(options.Device!);
        using var locator = new EsclServiceLocator(service =>
        {
            if (service.Uuid == deviceUuid)
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
        if (doc.ContentType == ContentTypes.PDF)
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

        var width = (int) Math.Round(options.PageSize!.WidthInInches * 300);
        var height = (int) Math.Round(options.PageSize!.HeightInInches * 300);
        if (inputCaps.MaxWidth is > 0)
        {
            width = Math.Min(width, inputCaps.MaxWidth.Value);
        }
        if (inputCaps.MaxHeight is > 0)
        {
            height = Math.Min(height, inputCaps.MaxHeight.Value);
        }

        return new EsclScanSettings
        {
            Width = width,
            Height = height,
            XResolution = dpi,
            YResolution = dpi,
            ColorMode = colorMode,
            InputSource = inputSource,
            Duplex = duplex,
            DocumentFormat = options.BitDepth == BitDepth.BlackAndWhite || options.MaxQuality
                ? ContentTypes.PDF // TODO: Use PNG if available?
                : ContentTypes.JPEG,
            XOffset = options.PageAlign switch
            {
                HorizontalAlign.Left => inputCaps.MaxWidth is > 0 ? inputCaps.MaxWidth.Value - width : 0,
                HorizontalAlign.Center => inputCaps.MaxWidth is > 0 ? (inputCaps.MaxWidth.Value - width) / 2 : 0,
                _ => 0
            }
            // TODO: Brightness/contrast, quality, etc.
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