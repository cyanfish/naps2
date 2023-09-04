using System.Net;
using System.Text;
using System.Xml.Linq;

namespace NAPS2.Escl.Client;

public class EsclClient
{
    private static readonly XNamespace ScanNs = EsclXmlHelper.ScanNs;
    private static readonly XNamespace PwgNs = EsclXmlHelper.PwgNs;
    private static readonly HttpClient HttpClient = new();
    private static readonly HttpClient ChunkedHttpClient = new()
        { DefaultRequestHeaders = { TransferEncodingChunked = true } };

    private readonly EsclService _service;

    public EsclClient(EsclService service)
    {
        _service = service;
    }

    public async Task<EsclCapabilities> GetCapabilities()
    {
        var doc = await DoRequest("ScannerCapabilities");
        var root = doc.Root;
        if (root?.Name != ScanNs + "ScannerCapabilities")
        {
            throw new InvalidOperationException("Unexpected root element: " + doc.Root?.Name);
        }
        var settingProfilesEl = root.Element(ScanNs + "SettingProfiles");
        var settingProfiles = new Dictionary<string, EsclSettingProfile>();
        if (settingProfilesEl != null)
        {
            foreach (var el in settingProfilesEl.Elements(ScanNs + "SettingProfile"))
            {
                ParseSettingProfile(el, settingProfiles);
            }
        }
        return new EsclCapabilities
        {
            Version = root.Element(PwgNs + "Version")?.Value,
            MakeAndModel = root.Element(PwgNs + "MakeAndModel")?.Value,
            SerialNumber = root.Element(PwgNs + "SerialNumber")?.Value,
            Uuid = root.Element(ScanNs + "UUID")?.Value,
            AdminUri = root.Element(ScanNs + "AdminURI")?.Value,
            IconUri = root.Element(ScanNs + "IconURI")?.Value
        };
    }

    public async Task<EsclScannerStatus> GetStatus()
    {
        var doc = await DoRequest("ScannerStatus");
        return new EsclScannerStatus();
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
                    new XElement(ScanNs + "Brightness", settings.Brightness),
                    new XElement(ScanNs + "Contrast", settings.Contrast),
                    new XElement(ScanNs + "Threshold", settings.Threshold),
                    new XElement(PwgNs + "DocumentFormat", settings.DocumentFormat)));
        var content = new StringContent(doc, Encoding.UTF8, "text/xml");
        var response = await HttpClient.PostAsync(GetUrl("ScanJobs"), content);
        response.EnsureSuccessStatusCode();
        return new EsclJob
        {
            Uri = response.Headers.Location!
        };
    }

    public async Task<RawDocument?> NextDocument(EsclJob job)
    {
        // TODO: Maybe check Content-Location on the response header to ensure no duplicate document?
        var response = await ChunkedHttpClient.GetAsync(job.Uri + "/NextDocument");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        return new RawDocument
        {
            Data = await response.Content.ReadAsByteArrayAsync(),
            ContentType = response.Content.Headers.ContentType?.MediaType
        };
    }

    private EsclSettingProfile ParseSettingProfile(XElement element,
        Dictionary<string, EsclSettingProfile> profilesDict)
    {
        var profileRef = element.Attribute("ref")?.Value;
        if (profileRef != null)
        {
            return profilesDict[profileRef];
        }
        var profile = new EsclSettingProfile
        {
            Name = element.Attribute("name")?.Value,
            ColorModes =
                ParseEnumValues<EsclColorMode>(element.Element(ScanNs + "ColorModes")?.Elements(ScanNs + "ColorMode"))
        };
        if (profile.Name != null)
        {
            profilesDict[profile.Name] = profile;
        }
        return profile;
    }

    private List<T> ParseEnumValues<T>(IEnumerable<XElement>? elements) where T : struct
    {
        var list = new List<T>();
        if (elements == null)
        {
            return list;
        }
        foreach (var el in elements)
        {
            if (Enum.TryParse<T>(el.Value, out var parsed))
            {
                list.Add(parsed);
            }
        }
        return list;
    }

    private async Task<XDocument> DoRequest(string endpoint)
    {
        // TODO: Retry logic
        var text = await HttpClient.GetStringAsync(GetUrl(endpoint));
        return XDocument.Parse(text);
    }

    private string GetUrl(string endpoint)
    {
        var protocol = _service.Tls ? "https" : "http";
        return new UriBuilder(protocol, (_service.IpV6 ?? _service.IpV4)!.ToString(), _service.Port,
                $"{_service.RootUrl}/{endpoint}")
            .Uri.ToString();
    }
}