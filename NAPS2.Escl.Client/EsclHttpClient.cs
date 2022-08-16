using System.Xml.Linq;

namespace NAPS2.Escl.Client;

public class EsclHttpClient
{
    private static readonly XNamespace ScanNs = XNamespace.Get("http://schemas.hp.com/imaging/escl/2011/05/03");
    private static readonly XNamespace PwgNs = XNamespace.Get("http://www.pwg.org/schemas/2010/12/sm");

    private readonly EsclService _service;

    public EsclHttpClient(EsclService service)
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
        // TODO: We're supposed to reuse these, right?
        var httpClient = new HttpClient();
        var protocol = _service.Tls ? "https" : "http";
        var url = $"{protocol}://{_service.Ip}:{_service.Port}/{_service.RootUrl}/{endpoint}";
        var response = await httpClient.GetAsync(url);
        // TODO: Handle status codes better
        response.EnsureSuccessStatusCode();
        var responseText = await response.Content.ReadAsStringAsync();
        return XDocument.Parse(responseText);
    }
}