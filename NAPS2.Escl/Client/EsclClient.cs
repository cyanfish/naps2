using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NAPS2.Escl.Client;

public class EsclClient
{
    private static readonly XNamespace ScanNs = EsclXmlHelper.ScanNs;
    private static readonly XNamespace PwgNs = EsclXmlHelper.PwgNs;

    // Client that verifies HTTPS certificates
    private static readonly HttpMessageHandler VerifiedHttpClientHandler = new StandardSocketsHttpHandler
    {
        MaxConnectionsPerServer = 256,
        ConnectTimeout = TimeSpan.FromSeconds(5)
    };
    private static readonly HttpClient VerifiedHttpClient = new(VerifiedHttpClientHandler)
    {
        Timeout = TimeSpan.FromSeconds(10)
    };
    private static readonly HttpClient LongTimeoutVerifiedHttpClient = new(VerifiedHttpClientHandler)
    {
        Timeout = TimeSpan.FromSeconds(120)
    };

    // Client that doesn't verify HTTPS certificates
    private static readonly HttpMessageHandler UnverifiedHttpClientHandler = new StandardSocketsHttpHandler
    {
        MaxConnectionsPerServer = 256,
        ConnectTimeout = TimeSpan.FromSeconds(5),
        SslOptions =
        {
            // ESCL certificates are generally self-signed - we aren't trying to verify server authenticity, just ensure
            // that the connection is encrypted and protect against passive interception.
            RemoteCertificateValidationCallback = (_, _, _, _) => true
        }
    };
    private static readonly HttpClient UnverifiedHttpClient = new(UnverifiedHttpClientHandler)
    {
        Timeout = TimeSpan.FromSeconds(10)
    };
    private static readonly HttpClient LongTimeoutUnverifiedHttpClient = new(UnverifiedHttpClientHandler)
    {
        Timeout = TimeSpan.FromSeconds(120)
    };

    private readonly EsclService _service;
    private bool _httpFallback;

    public EsclClient(EsclService service)
    {
        _service = service;
    }

    public EsclSecurityPolicy SecurityPolicy { get; set; }

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public CancellationToken CancelToken { get; set; }

    private HttpClient HttpClient => SecurityPolicy.HasFlag(EsclSecurityPolicy.ClientRequireTrustedCertificate)
        ? VerifiedHttpClient
        : UnverifiedHttpClient;

    private HttpClient LongTimeoutHttpClient =>
        SecurityPolicy.HasFlag(EsclSecurityPolicy.ClientRequireTrustedCertificate)
            ? LongTimeoutVerifiedHttpClient
            : LongTimeoutUnverifiedHttpClient;

    public async Task<EsclCapabilities> GetCapabilities()
    {
        var doc = await DoRequest("ScannerCapabilities");
        return CapabilitiesParser.Parse(doc);
    }

    public async Task<EsclScannerStatus> GetStatus()
    {
        var doc = await DoRequest("ScannerStatus");
        var root = doc.Root;
        if (root?.Name != ScanNs + "ScannerStatus")
        {
            throw new InvalidOperationException("Unexpected root element: " + doc.Root?.Name);
        }
        var jobStates = new Dictionary<string, EsclJobState>();
        foreach (var jobInfoEl in root.Element(ScanNs + "Jobs")?.Elements(ScanNs + "JobInfo") ?? [])
        {
            var jobUri = jobInfoEl.Element(PwgNs + "JobUri")?.Value;
            var jobState = ParseHelper.MaybeParseEnum(jobInfoEl.Element(PwgNs + "JobState"), EsclJobState.Unknown);
            if (jobUri != null && jobState != EsclJobState.Unknown)
            {
                jobStates.Add(jobUri, jobState);
            }
        }
        return new EsclScannerStatus
        {
            State = ParseHelper.MaybeParseEnum(root.Element(PwgNs + "State"), EsclScannerState.Unknown),
            AdfState = ParseHelper.MaybeParseEnum(root.Element(ScanNs + "AdfState"), EsclAdfState.Unknown),
            JobStates = jobStates
        };
    }

    public async Task<EsclJob> CreateScanJob(EsclScanSettings settings)
    {
        var doc =
            EsclXmlHelper.CreateDocAsString(
                new XElement(ScanNs + "ScanSettings",
                    new XElement(PwgNs + "Version", "2.0"),
                    new XElement(ScanNs + "Intent", "TextAndGraphic"),
                    new XElement(PwgNs + "ScanRegions",
                        new XAttribute(PwgNs + "MustHonor", "true"),
                        new XElement(PwgNs + "ScanRegion",
                            new XElement(PwgNs + "Height", settings.Height),
                            new XElement(PwgNs + "ContentRegionUnits", "escl:ThreeHundredthsOfInches"),
                            new XElement(PwgNs + "Width", settings.Width),
                            new XElement(PwgNs + "XOffset", settings.XOffset),
                            new XElement(PwgNs + "YOffset", settings.YOffset))),
                    new XElement(PwgNs + "InputSource", settings.InputSource),
                    new XElement(ScanNs + "Duplex", settings.Duplex),
                    new XElement(ScanNs + "ColorMode", settings.ColorMode),
                    new XElement(ScanNs + "XResolution", settings.XResolution),
                    new XElement(ScanNs + "YResolution", settings.YResolution),
                    // TODO: Brightness/contrast/threshold
                    // new XElement(ScanNs + "Brightness", settings.Brightness),
                    // new XElement(ScanNs + "Contrast", settings.Contrast),
                    // new XElement(ScanNs + "Threshold", settings.Threshold),
                    OptionalElement(ScanNs + "CompressionFactor", settings.CompressionFactor),
                    new XElement(PwgNs + "DocumentFormat", settings.DocumentFormat)));
        var content = new StringContent(doc, Encoding.UTF8, "text/xml");
        var response = await WithHttpFallback(
            () => GetUrl($"/{_service.RootUrl}/ScanJobs"),
            url =>
            {
                Logger.LogDebug("ESCL POST {Url}", url);
                return HttpClient.PostAsync(url, content);
            });
        response.EnsureSuccessStatusCode();
        Logger.LogDebug("POST OK");

        var uri = response.Headers.Location!;

        return new EsclJob
        {
            UriPath = uri.IsAbsoluteUri ? uri.AbsolutePath : uri.OriginalString
        };
    }

    private XElement? OptionalElement(XName elementName, int? value)
    {
        if (value == null) return null;
        return new XElement(elementName, value);
    }

    public async Task<RawDocument?> NextDocument(EsclJob job, Action<double>? pageProgress = null, bool shortTimeout = false)
    {
        var progressCts = new CancellationTokenSource();
        if (pageProgress != null)
        {
            var progressUrl = GetUrl($"{job.UriPath}/Progress");
            var progressResponse = await LongTimeoutHttpClient.GetStreamAsync(progressUrl);
            var streamReader = new StreamReader(progressResponse);
            _ = Task.Run(async () =>
            {
                using var streamReaderForDisposal = streamReader;
                while (await streamReader.ReadLineAsync() is { } line)
                {
                    if (progressCts.IsCancellationRequested)
                    {
                        return;
                    }
                    if (double.TryParse(line, NumberStyles.Any, CultureInfo.InvariantCulture, out var progress))
                    {
                        pageProgress(progress);
                    }
                }
            });
        }
        try
        {
            // TODO: Maybe check Content-Location on the response header to ensure no duplicate document?
            HttpResponseMessage response;
            while (true)
            {
                response = await WithHttpFallback(
                    () => GetUrl($"{job.UriPath}/NextDocument"),
                    url =>
                    {
                        Logger.LogDebug("ESCL GET {Url}", url);
                        var client = shortTimeout ? HttpClient : LongTimeoutHttpClient;
                        return client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    });
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    // ServiceUnavailable = retry after a delay
                    Logger.LogDebug("GET returned 503, waiting to retry");
                    await Task.Delay(2000);
                    continue;
                }
                if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
                {
                    // NotFound = end of scan, Gone = canceled
                    Logger.LogDebug("GET failed: {Status}", response.StatusCode);
                    return null;
                }
                response.EnsureSuccessStatusCode();
                break;
            }
            // TODO: Define a NAPS2 protocol extension to shorten this timeout to 10s (once we do the rollout of server-side 503s)
            var data = await ReadStreamWithPerReadTimeout(await response.Content.ReadAsStreamAsync(), 60_000);
            var doc = new RawDocument
            {
                Data = data,
                ContentType = response.Content.Headers.ContentType?.MediaType,
                ContentLocation = response.Content.Headers.ContentLocation?.ToString()
            };
            if (doc.Data.Length == 0)
            {
                throw new Exception("ESCL response had no data, the connection may have been interrupted");
            }
            Logger.LogDebug("GET OK: {Type} ({Bytes} bytes) {Location}", doc.ContentType, doc.Data.Length,
                doc.ContentLocation);
            return doc;
        }
        finally
        {
            progressCts.Cancel();
        }
    }

    private async Task<byte[]> ReadStreamWithPerReadTimeout(Stream stream, int timeout)
    {
        // We expect the server to be continuously sending some kind of data - if reads take longer than the timeout,
        // we assume that the connection has been disrupted.
        MemoryStream tempStream = new MemoryStream();
        byte[] buffer = new byte[65536];
        while (true)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
            if (bytesRead == 0) break;
            tempStream.Write(buffer, 0, bytesRead);
        }
        return tempStream.ToArray();
    }

    public async Task<string> ErrorDetails(EsclJob job)
    {
        var response = await WithHttpFallback(
            () => GetUrl($"{job.UriPath}/ErrorDetails"),
            url =>
            {
                Logger.LogDebug("ESCL GET {Url}", url);
                return HttpClient.GetAsync(url);
            });
        response.EnsureSuccessStatusCode();
        Logger.LogDebug("GET OK");
        return await response.Content.ReadAsStringAsync();
    }

    public async Task CancelJob(EsclJob job)
    {
        var response = await WithHttpFallback(
            () => GetUrl(job.UriPath),
            url =>
            {
                Logger.LogDebug("ESCL DELETE {Url}", url);
                return HttpClient.DeleteAsync(url);
            });
        if (!response.IsSuccessStatusCode)
        {
            Logger.LogDebug("DELETE failed: {Status}", response.StatusCode);
            return;
        }
        response.EnsureSuccessStatusCode();
        Logger.LogDebug("DELETE OK");
    }

    private async Task<XDocument> DoRequest(string endpoint)
    {
        // TODO: Retry logic
        var response = await WithHttpFallback(
            () => GetUrl($"/{_service.RootUrl}/{endpoint}"),
            url =>
            {
                Logger.LogDebug("ESCL GET {Url}", url);
                return HttpClient.GetAsync(url, CancelToken);
            });
        response.EnsureSuccessStatusCode();
        Logger.LogDebug("GET OK");
        var text = await response.Content.ReadAsStringAsync();
        var doc = XDocument.Parse(text);
        return doc;
    }

    private async Task<T> WithHttpFallback<T>(Func<string> urlFunc, Func<string, Task<T>> func)
    {
        string url = urlFunc();
        try
        {
            return await func(url);
        }
        catch (HttpRequestException ex) when (!SecurityPolicy.HasFlag(EsclSecurityPolicy.ClientRequireHttps) &&
                                              !_httpFallback &&
                                              url.StartsWith("https://") && (
                                                  ex.InnerException is AuthenticationException ||
                                                  ex.InnerException?.InnerException is AuthenticationException))
        {
            Logger.LogDebug(ex, "TLS authentication error; falling back to HTTP");
            _httpFallback = true;
            url = urlFunc();
            return await func(url);
        }
    }

    private string GetUrl(string endpoint)
    {
        bool tls = (_service.Tls || _service.Port == 443) && !_httpFallback &&
                   !SecurityPolicy.HasFlag(EsclSecurityPolicy.ClientDisableHttps);
        if (SecurityPolicy.HasFlag(EsclSecurityPolicy.ClientRequireHttps) && !tls)
        {
            throw new EsclSecurityPolicyViolationException(
                $"EsclSecurityPolicy of {SecurityPolicy} doesn't allow HTTP connections");
        }
        var protocol = tls ? "https" : "http";
        return $"{protocol}://{GetHostAndPort(_service.Tls && !_httpFallback)}{endpoint}";
    }

    private string GetHostAndPort(bool tls)
    {
        var port = tls ? _service.TlsPort : _service.Port;
        var host = new IPEndPoint(_service.RemoteEndpoint, port).ToString();
#if NET6_0_OR_GREATER
        if (OperatingSystem.IsMacOS())
        {
            // Using the mDNS hostname is more reliable on Mac (but doesn't work at all on Windows)
            host = $"{_service.Host}:{port}";
        }
#endif
        return host;
    }
}