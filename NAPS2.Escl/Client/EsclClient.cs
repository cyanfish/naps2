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

    // Sadly as we're still using .NET Framework on Windows, we're stuck with the old HttpClient implementation, which
    // has trouble with concurrency. So we use a separate client for long running requests (Progress/NextDocument).
    private static readonly HttpClient HttpClient = new();
    private static readonly HttpClient ProgressHttpClient = new();
    private static readonly HttpClient DocumentHttpClient = new();

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
        return new EsclScannerStatus
        {
            State = Enum.TryParse<EsclScannerState>(root!.Element(PwgNs + "State")?.Value, out var state)
                ? state
                : EsclScannerState.Unknown,
            AdfState = Enum.TryParse<EsclAdfState>(root.Element(ScanNs + "AdfState")?.Value, out var adfState)
                ? adfState
                : EsclAdfState.Unknown
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
                    new XElement(PwgNs + "DocumentFormat", settings.DocumentFormat)));
        var content = new StringContent(doc, Encoding.UTF8, "text/xml");
        var url = GetUrl("ScanJobs");
        Logger.LogDebug("ESCL POST {Url}", url);
        Logger.LogDebug("{Doc}", doc);
        var response = await HttpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        Logger.LogDebug("POST OK");
        return new EsclJob
        {
            Uri = response.Headers.Location!
        };
    }

    public async Task<RawDocument?> NextDocument(EsclJob job, Action<double>? pageProgress = null)
    {
        if (pageProgress != null)
        {
            var progressUrl = job.Uri + "/Progress";
            var progressResponse = await ProgressHttpClient.GetStreamAsync(progressUrl);
            var streamReader = new StreamReader(progressResponse);
            _ = Task.Run(async () =>
            {
                while (await streamReader.ReadLineAsync() is { } line)
                {
                    if (double.TryParse(line, NumberStyles.Any, CultureInfo.InvariantCulture, out var progress))
                    {
                        pageProgress(progress);
                    }
                }
            });
        }
        // TODO: Maybe check Content-Location on the response header to ensure no duplicate document?
        var url = job.Uri + "/NextDocument";
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
        Logger.LogDebug("{Type} ({Bytes} bytes) {Location}", doc.ContentType, doc.Data.Length, doc.ContentLocation);
        return doc;
    }

    public async Task CancelJob(EsclJob job)
    {
        Logger.LogDebug("ESCL DELETE {Url}", job.Uri);
        var response = await HttpClient.DeleteAsync(job.Uri);
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
        var url = GetUrl(endpoint);
        Logger.LogDebug("ESCL GET {Url}", url);
        var response = await HttpClient.GetAsync(url, CancelToken);
        response.EnsureSuccessStatusCode();
        var text = await response.Content.ReadAsStringAsync();
        var doc = XDocument.Parse(text);
        Logger.LogDebug("{Doc}", doc);
        return doc;
    }

    private string GetUrl(string endpoint)
    {
        var protocol = _service.Tls ? "https" : "http";
        var ipAndPort = new IPEndPoint(_service.RemoteEndpoint, _service.Port).ToString();
#if NET6_0_OR_GREATER
        if (OperatingSystem.IsMacOS())
        {
            // Using the mDNS hostname is more reliable on Mac (but doesn't work at all on Windows)
            ipAndPort = $"{_service.Host}:{_service.Port}";
        }
#endif
        return $"{protocol}://{ipAndPort}/{_service.RootUrl}/{endpoint}";
    }
}