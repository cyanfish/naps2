using System.Collections.Immutable;
using System.Net.Http;
using System.Net.Sockets;
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

    private static string GetUuid(ScanDevice device)
    {
        var parts = device.ID.Split('|');
        if (parts.Length == 1)
        {
            // Current IDs are just the UUID
            return parts[0];
        }
        if (parts.Length != 2)
        {
            throw new ArgumentException("Invalid ESCL device ID");
        }
        // Old IDs have both the IP and UUID separated by "|"
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
            // TODO: Consider limiting available devices by security policy
            var ip = service.IpV4 ?? service.IpV6!;
            if (options.ExcludeLocalIPs && localIPsTask!.Result.Contains(ip.ToString()))
            {
                return;
            }
            var id = service.Uuid;
            var name = string.IsNullOrEmpty(service.ScannerName)
                ? $"{ip}"
                : $"{service.ScannerName} ({ip})";
            var client = new EsclClient(service);
            callback(new ScanDevice(Driver.Escl, id, name, client.IconUri, client.ConnectionUri));
        });
        locator.Logger = _logger;
        locator.Start();
        try
        {
            await Task.Delay(options.EsclOptions.SearchTimeout, cancelToken);
        }
        catch (TaskCanceledException)
        {
        }
    }

    public async Task<ScanCaps> GetCaps(ScanOptions options, CancellationToken cancelToken)
    {
        if (cancelToken.IsCancellationRequested) return new ScanCaps();

        try
        {
            var (client, caps) = await GetEsclClientWithCaps(options, cancelToken, ScanEvents.Stub);
            if (client == null || caps == null) return new ScanCaps();
            return new ScanCaps
            {
                MetadataCaps = new()
                {
                    Model = caps.MakeAndModel,
                    Manufacturer = caps.Manufacturer,
                    SerialNumber = caps.SerialNumber,
                    IconUri = client.IconUri
                },
                PaperSourceCaps = new()
                {
                    SupportsFlatbed = caps.PlatenCaps != null,
                    SupportsFeeder = caps.AdfSimplexCaps != null,
                    SupportsDuplex = caps.AdfDuplexCaps != null,
                    CanCheckIfFeederHasPaper = true
                },
                FlatbedCaps = MapCaps(caps.PlatenCaps),
                FeederCaps = MapCaps(caps.AdfSimplexCaps),
                DuplexCaps = MapCaps(caps.AdfDuplexCaps)
            };
        }
        catch (HttpRequestException ex) when (ex.InnerException is TaskCanceledException or SocketException)
        {
            // A connection timeout manifests as TaskCanceledException
            _logger.LogError(ex, "Error connecting to ESCL device");
            throw new DeviceCommunicationException();
        }
        catch (TaskCanceledException)
        {
        }
        return new ScanCaps();
    }

    private PerSourceCaps? MapCaps(EsclInputCaps? caps)
    {
        if (caps == null)
        {
            return null;
        }
        return PerSourceCaps.UnionAll(caps.SettingProfiles.Select(profile => MapSettingProfile(caps, profile)));
    }

    private PerSourceCaps MapSettingProfile(EsclInputCaps caps, EsclSettingProfile profile)
    {
        DpiCaps? dpiCaps = null;
        if (profile.DiscreteResolutions.Count > 0)
        {
            dpiCaps = new DpiCaps
            {
                Values = profile.DiscreteResolutions
                    .Where(res => res.XResolution == res.YResolution)
                    .Select(res => res.XResolution).ToImmutableList()
            };
        }
        else if (profile.XResolutionRange != null && profile.YResolutionRange != null)
        {
            int min = Math.Max(profile.XResolutionRange.Min, profile.YResolutionRange.Min);
            int max = Math.Min(profile.XResolutionRange.Max, profile.YResolutionRange.Max);
            int step = Math.Max(profile.XResolutionRange.Step, profile.YResolutionRange.Step);
            dpiCaps = DpiCaps.ForRange(min, max, step);
        }
        return new PerSourceCaps
        {
            DpiCaps = dpiCaps,
            BitDepthCaps = new BitDepthCaps
            {
                SupportsColor = profile.ColorModes.Contains(EsclColorMode.RGB24),
                SupportsGrayscale = profile.ColorModes.Contains(EsclColorMode.Grayscale8),
                SupportsBlackAndWhite = profile.ColorModes.Contains(EsclColorMode.BlackAndWhite1)
            },
            PageSizeCaps = caps.MaxWidth != null && caps.MaxHeight != null
                ? new PageSizeCaps
                {
                    ScanArea = new PageSize(caps.MaxWidth.Value / 300m, caps.MaxHeight.Value / 300m, PageSizeUnit.Inch)
                }
                : null
        };
    }

    public async Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<IMemoryImage> callback)
    {
        if (cancelToken.IsCancellationRequested) return;

        try
        {
            var (client, caps) = await GetEsclClientWithCaps(options, cancelToken, scanEvents);
            if (client == null || caps == null) return;
            var status = await client.GetStatus();
            bool hasProgressExtension = caps.Naps2Extensions?.Contains("Progress") ?? false;
            bool hasErrorDetailsExtension = caps.Naps2Extensions?.Contains("ErrorDetails") ?? false;
            bool hasShortTimeoutExtension = caps.Naps2Extensions?.Contains("ShortTimeout") ?? false;
            bool hasAnyDpiExtension = caps.Naps2Extensions?.Contains("AnyDpi") ?? false;
            var scanSettings = GetScanSettings(options, caps, hasAnyDpiExtension);
            Action<double>? progressCallback = hasProgressExtension ? scanEvents.PageProgress : null;

            if (cancelToken.IsCancellationRequested) return;

            VerifyStatus(status, scanSettings);

            var job = await CreateScanJobAndCorrectInvalidSettings(client, scanSettings);

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
                    var doc = await GetNextDocumentWithRetries(client, job, progressCallback, hasShortTimeoutExtension);
                    if (doc == null) break;
                    foreach (var image in GetImagesFromRawDocument(options, doc))
                    {
                        callback(image);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "ESCL driver error");
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
        catch (HttpRequestException ex) when (ex.InnerException is TaskCanceledException or SocketException)
        {
            // A connection timeout manifests as TaskCanceledException
            _logger.LogError(ex, "Error connecting to ESCL device");
            throw new DeviceCommunicationException();
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async Task<(EsclClient?, EsclCapabilities?)> GetEsclClientWithCaps(ScanOptions options,
        CancellationToken cancelToken, IScanEvents scanEvents)
    {
        EsclClient client;
        string deviceId = options.Device!.ID;

        void SetUpClient()
        {
            client.SecurityPolicy = options.EsclOptions.SecurityPolicy;
            client.Logger = _logger;
            client.CancelToken = cancelToken;
        }
        void MaybeSendNewUris()
        {
            string? iconUri = client.IconUri;
            string connectionUri = client.ConnectionUri;
            if (iconUri != options.Device.IconUri || connectionUri != options.Device.ConnectionUri)
            {
                scanEvents.DeviceUriChanged(iconUri, connectionUri);
            }
        }

        // If we only have a URI to connect, just use it.
        if (deviceId.StartsWith("http://") || deviceId.StartsWith("https://"))
        {
            client = new EsclClient(new Uri(deviceId));
            SetUpClient();
            return (client, await client.GetCapabilities());
        }

        // If we have both a UUID and a ConnectionUri, race an mDNS request with a GetCapabilities request.
        // This is the best of both worlds:
        // - If the connection info is up to date, we connect directly immediately.
        // - If the IP has changed, we find out and fall back to that
        // TODO: Maybe racing [GetCapabilities] with [mDNS + GetCapabilities] is slightly more optimal
        var serviceTask = FindDeviceEsclService(options, cancelToken);
        if (options.Device!.ConnectionUri != null)
        {
            client = new EsclClient(new Uri(options.Device.ConnectionUri));
            SetUpClient();
            var capsTask = client.GetCapabilities();
            await Task.WhenAny(capsTask, serviceTask);
            if (capsTask.Status == TaskStatus.RanToCompletion)
            {
                return (client, await capsTask);
            }
        }

        // If we have no known connection info (or failed to connect with it), use the mDNS response for the client
        var service = await serviceTask;
        if (cancelToken.IsCancellationRequested) return (null, null);
        if (service == null) throw new DeviceOfflineException();
        client = new EsclClient(service);
        MaybeSendNewUris();
        SetUpClient();
        return (client, await client.GetCapabilities());
    }

    private async Task<EsclJob> CreateScanJobAndCorrectInvalidSettings(EsclClient client, EsclScanSettings scanSettings)
    {
        _logger.LogDebug("Creating ESCL job: format {Format}, source {Source}, mode {Mode}",
            scanSettings.DocumentFormat, scanSettings.InputSource, scanSettings.ColorMode);
        EsclJob job;
        try
        {
            job = await client.CreateScanJob(scanSettings);
        }
        catch (HttpRequestException ex) when (scanSettings.ColorMode == EsclColorMode.BlackAndWhite1 &&
                                              ex.Message.Contains("409 (Conflict)"))
        {
            scanSettings = scanSettings with
            {
                ColorMode = EsclColorMode.Grayscale8,
                DocumentFormat = ContentTypes.JPEG
            };
            _logger.LogDebug("Scanning in Grayscale instead of Black & White due to HTTP 409 response");
            job = await client.CreateScanJob(scanSettings);
        }
        return job;
    }

    private async Task<RawDocument?> GetNextDocumentWithRetries(EsclClient client, EsclJob job,
        Action<double>? progress, bool shortTimeout)
    {
        int retries = 0;
        while (true)
        {
            try
            {
                return await client.NextDocument(job, progress, shortTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "ESCL NextDocument error");
                if (++retries > MAX_DOCUMENT_TRIES)
                {
                    _logger.LogDebug("ESCL NextDocument failed, no more retries left");
                    throw;
                }
                EsclJobState jobState = EsclJobState.Unknown;
                try
                {
                    var status = await client.GetStatus();
                    jobState = status.JobStates.Get(job.UriPath);
                }
                catch (Exception)
                {
                    _logger.LogDebug("ESCL GetStatus failed, could not get job state");
                }
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
            throw new DeviceBusyException();
        }
        if (status.State == EsclScannerState.Down)
        {
            throw new DeviceOfflineException();
        }
        if (scanSettings.InputSource == EsclInputSource.Feeder)
        {
            if (status.AdfState == EsclAdfState.ScannerAdfEmpty)
            {
                throw new DeviceFeederEmptyException();
            }
            if (status.AdfState == EsclAdfState.ScannerAdfJam)
            {
                throw new DevicePaperJamException();
            }
            if (status.AdfState is not (EsclAdfState.Unknown or EsclAdfState.ScannerAdfProcessing
                or EsclAdfState.ScannedAdfLoaded))
            {
                throw new DeviceException(status.AdfState.ToString());
            }
        }
    }

    private async Task<EsclService?> FindDeviceEsclService(ScanOptions options, CancellationToken cancelToken)
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
        Task.Delay(options.EsclOptions.SearchTimeout, cancelToken)
            .ContinueWith(_ => foundTcs.TrySetResult(null)).AssertNoAwait();
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

    private EsclScanSettings GetScanSettings(ScanOptions options, EsclCapabilities caps, bool hasAnyDpiExtension)
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

            if (!hasAnyDpiExtension)
            {
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

            _logger.LogDebug("ESCL setting profile supports formats: {Formats}",
                string.Join(",", settingProfile.DocumentFormats.Concat(settingProfile.DocumentFormatsExt).Distinct()));
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

        var contentType = ContentTypes.JPEG;
        if (options.BitDepth == BitDepth.BlackAndWhite || options.MaxQuality)
        {
            bool supportsPng = settingProfile != null && settingProfile.DocumentFormats
                .Concat(settingProfile.DocumentFormatsExt).Contains(ContentTypes.PNG);
            contentType = supportsPng ? ContentTypes.PNG : ContentTypes.PDF;
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
            DocumentFormat = contentType,
            XOffset = options.PageAlign switch
            {
                HorizontalAlign.Left => inputCaps.MaxWidth is > 0 ? inputCaps.MaxWidth.Value - width : 0,
                HorizontalAlign.Center => inputCaps.MaxWidth is > 0 ? (inputCaps.MaxWidth.Value - width) / 2 : 0,
                _ => 0
            },
            CompressionFactor = caps.CompressionFactorSupport is { Min: 0, Max: 100, Step: 1 } ? options.Quality : null
            // TODO: Brightness/contrast, etc.
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