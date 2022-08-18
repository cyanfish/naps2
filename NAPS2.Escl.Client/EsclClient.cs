using System.Xml.Linq;

namespace NAPS2.Escl.Client;

public class EsclClient
{
    private static readonly XNamespace ScanNs = EsclXmlHelper.ScanNs;
    private static readonly XNamespace PwgNs = EsclXmlHelper.PwgNs;

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
        var text = await new HttpClient().GetStringAsync(GetUrl("ScannerStatus"));
        var doc = XDocument.Parse(text);
        return new EsclScannerStatus();
    }

    public async Task<EsclJob> CreateScanJob(EsclScanSettings scanSettings)
    {
        var doc =
            EsclXmlHelper.CreateDocAsString(
                new XElement(ScanNs + "ScanSettings",
                    new XElement(PwgNs + "Version", "2.6"),
                    new XElement(ScanNs + "Intent", "Photo"),
                    new XElement(PwgNs + "ScanRegions",
                        new XElement(PwgNs + "ScanRegion",
                            new XElement(PwgNs + "Height", "1200"),
                            new XElement(PwgNs + "ContentRegionUnits", "escl:ThreeHundredthsOfInches"),
                            new XElement(PwgNs + "Width", "1800"),
                            new XElement(PwgNs + "XOffset"),
                            new XElement(PwgNs + "YOffset"))),
                    new XElement(PwgNs + "InputSource", "Platen"),
                    new XElement(ScanNs + "ColorMode", "Grayscale8")));
        var response = await new HttpClient().PostAsync(GetUrl("ScanJobs"), new StringContent(doc));
        response.EnsureSuccessStatusCode();
        return new EsclJob
        {
            Uri = response.Headers.Location
        };
    }

    public async Task<byte[]> NextDocument(EsclJob job)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.TransferEncodingChunked = true;
        // TODO: Maybe check Content-Location on the response header to ensure no duplicate document?
        return await client.GetByteArrayAsync(job.Uri + "/NextDocument");
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
        // TODO: We're supposed to reuse HttpClient, right?
        var text = await new HttpClient().GetStringAsync(GetUrl(endpoint));
        return XDocument.Parse(text);
    }

    private string GetUrl(string endpoint)
    {
        var protocol = _service.Tls ? "https" : "http";
        return new UriBuilder(protocol, _service.Ip.ToString(), _service.Port, $"{_service.RootUrl}/{endpoint}")
            .Uri.ToString();
    }
}