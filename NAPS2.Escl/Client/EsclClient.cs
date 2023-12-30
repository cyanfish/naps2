using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NAPS2.Escl.Client;

public class EsclClient
{
    private static readonly XNamespace ScanNs = EsclXmlHelper.ScanNs;
    private static readonly XNamespace PwgNs = EsclXmlHelper.PwgNs;

    private static readonly HttpClientHandler HttpClientHandler = new()
    {
        // ESCL certificates are generally self-signed - we aren't trying to verify server authenticity, just ensure
        // that the connection is encrypted and protect against passive interception.
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
    };
    // Sadly as we're still using .NET Framework on Windows, we're stuck with the old HttpClient implementation, which
    // has trouble with concurrency. So we use a separate client for long running requests (Progress/NextDocument).
    private static readonly HttpClient HttpClient = new(HttpClientHandler);
    private static readonly HttpClient ProgressHttpClient = new(HttpClientHandler);
    private static readonly HttpClient DocumentHttpClient = new(HttpClientHandler);

    private readonly EsclService _service;

    public EsclClient(EsclService service)
    {
        _service = service;
    }

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public CancellationToken CancelToken { get; set; }

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
        var url = GetUrl($"/{_service.RootUrl}/ScanJobs");
        Logger.LogDebug("ESCL POST {Url}", url);
        var response = await HttpClient.PostAsync(url, content);
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

    public async Task<RawDocument?> NextDocument(EsclJob job, Action<double>? pageProgress = null)
    {
        var progressCts = new CancellationTokenSource();
        if (pageProgress != null)
        {
            var progressUrl = GetUrl($"{job.UriPath}/Progress");
            var progressResponse = await ProgressHttpClient.GetStreamAsync(progressUrl);
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
            var url = GetUrl($"{job.UriPath}/NextDocument");
            Logger.LogDebug("ESCL GET {Url}", url);
            var response = await DocumentHttpClient.GetAsync(url);
            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
            {
                // NotFound = end of scan, Gone = canceled
                Logger.LogDebug("GET failed: {Status}", response.StatusCode);
                return null;
            }
            response.EnsureSuccessStatusCode();
            var doc = new RawDocument
            {
                Data = await response.Content.ReadAsByteArrayAsync(),
                ContentType = response.Content.Headers.ContentType?.MediaType,
                ContentLocation = response.Content.Headers.ContentLocation?.ToString()
            };
            Logger.LogDebug("GET OK: {Type} ({Bytes} bytes) {Location}", doc.ContentType, doc.Data.Length,
                doc.ContentLocation);
            return doc;
        }
        finally
        {
            progressCts.Cancel();
        }
    }

    public async Task<string> ErrorDetails(EsclJob job)
    {
        var url = GetUrl($"{job.UriPath}/ErrorDetails");
        Logger.LogDebug("ESCL GET {Url}", url);
        var response = await HttpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        Logger.LogDebug("GET OK");
        return await response.Content.ReadAsStringAsync();
    }

    public async Task CancelJob(EsclJob job)
    {
        var url = GetUrl(job.UriPath);
        Logger.LogDebug("ESCL DELETE {Url}", url);
        var response = await HttpClient.DeleteAsync(url);
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
        var url = GetUrl($"/{_service.RootUrl}/{endpoint}");
        Logger.LogDebug("ESCL GET {Url}", url);
        var response = await HttpClient.GetAsync(url, CancelToken);
        response.EnsureSuccessStatusCode();
        Logger.LogDebug("GET OK");
        var text = await response.Content.ReadAsStringAsync();
        var doc = XDocument.Parse(text);
        return doc;
    }

    private string GetUrl(string endpoint)
    {
        var protocol = _service.Tls || _service.Port == 443 ? "https" : "http";
        return $"{protocol}://{GetHostAndPort()}{endpoint}";
    }

    private string GetHostAndPort()
    {
        var host = new IPEndPoint(_service.RemoteEndpoint, _service.Port).ToString();
#if NET6_0_OR_GREATER
        if (OperatingSystem.IsMacOS())
        {
            // Using the mDNS hostname is more reliable on Mac (but doesn't work at all on Windows)
            host = $"{_service.Host}:{_service.Port}";
        }
#endif
        return host;
    }
}