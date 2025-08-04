using System.Xml.Linq;

namespace NAPS2.Escl.Client;

internal static class CapabilitiesParser
{
    private static readonly XNamespace ScanNs = EsclXmlHelper.ScanNs;
    private static readonly XNamespace PwgNs = EsclXmlHelper.PwgNs;

    public static EsclCapabilities Parse(XDocument doc)
    {
        var root = doc.Root;
        if (root?.Name.LocalName != "ScannerCapabilities" ||
            !root.Name.NamespaceName.StartsWith("http://schemas.hp.com/imaging/escl/"))
        {
            throw new InvalidOperationException("Unexpected root element: " + doc.Root?.Name);
        }
        var settingProfilesEl = root!.Element(ScanNs + "SettingProfiles");
        var settingProfiles = new Dictionary<string, EsclSettingProfile>();
        if (settingProfilesEl != null)
        {
            foreach (var el in settingProfilesEl.Elements(ScanNs + "SettingProfile"))
            {
                ParseSettingProfile(el, settingProfiles);
            }
        }
        var platenCapsEl = root.Element(ScanNs + "Platen")?.Element(ScanNs + "PlatenInputCaps");
        var adfSimplexCapsEl = root.Element(ScanNs + "Adf")?.Element(ScanNs + "AdfSimplexInputCaps");
        var adfDuplexCapsEl = root.Element(ScanNs + "Adf")?.Element(ScanNs + "AdfDuplexInputCaps");
        return new EsclCapabilities
        {
            Version = root.Element(PwgNs + "Version")?.Value ?? EsclCapabilities.DEFAULT_VERSION,
            MakeAndModel = root.Element(PwgNs + "MakeAndModel")?.Value,
            SerialNumber = root.Element(PwgNs + "SerialNumber")?.Value,
            Manufacturer = root.Element(ScanNs + "Manufacturer")?.Value,
            Uuid = root.Element(ScanNs + "UUID")?.Value,
            AdminUri = root.Element(ScanNs + "AdminURI")?.Value,
            IconUri = root.Element(ScanNs + "IconURI")?.Value,
            Naps2Extensions = root.Element(ScanNs + "Naps2Extensions")?.Value,
            PlatenCaps = ParseInputCaps(platenCapsEl, settingProfiles),
            AdfSimplexCaps = ParseInputCaps(adfSimplexCapsEl, settingProfiles),
            AdfDuplexCaps = ParseInputCaps(adfDuplexCapsEl, settingProfiles),
            CompressionFactorSupport = ParseRange(root.Element(ScanNs + "CompressionFactorSupport"))
        };
    }

    private static EsclSettingProfile ParseSettingProfile(XElement element,
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
                ParseEnumValues<EsclColorMode>(element.Element(ScanNs + "ColorModes")?.Elements(ScanNs + "ColorMode")),
            DocumentFormats = element.Element(ScanNs + "DocumentFormats")?.Elements(PwgNs + "DocumentFormat")
                .Select(x => x.Value).ToList() ?? new List<string>(),
            DocumentFormatsExt = element.Element(ScanNs + "DocumentFormats")?.Elements(PwgNs + "DocumentFormatExt")
                .Select(x => x.Value).ToList() ?? new List<string>(),
            DiscreteResolutions = ParseDiscreteResolutions(element.Element(ScanNs + "SupportedResolutions")
                ?.Element(ScanNs + "DiscreteResolutions")?.Elements(ScanNs + "DiscreteResolution")),
            XResolutionRange = ParseRange(element.Element(ScanNs + "SupportedResolutions")
                ?.Element(ScanNs + "XResolutionRange")),
            YResolutionRange = ParseRange(element.Element(ScanNs + "SupportedResolutions")
                ?.Element(ScanNs + "YResolutionRange")),
        };
        if (profile.Name != null)
        {
            profilesDict[profile.Name] = profile;
        }
        return profile;
    }

    private static EsclRange? ParseRange(XElement? element)
    {
        if (element == null) return null;

        var min = ParseHelper.MaybeParseInt(element.Element(ScanNs + "Min"));
        var max = ParseHelper.MaybeParseInt(element.Element(ScanNs + "Max"));
        var normal = ParseHelper.MaybeParseInt(element.Element(ScanNs + "Normal"));
        var step = ParseHelper.MaybeParseInt(element.Element(ScanNs + "Step"));
        if (min != null && max != null && normal != null)
        {
            return new EsclRange(min.Value, max.Value, normal.Value, step ?? 1);
        }
        return null;
    }

    private static List<DiscreteResolution> ParseDiscreteResolutions(IEnumerable<XElement>? elements)
    {
        var list = new List<DiscreteResolution>();
        if (elements == null)
        {
            return list;
        }
        foreach (var el in elements)
        {
            var xRes = el.Element(ScanNs + "XResolution");
            var yRes = el.Element(ScanNs + "YResolution");
            if (xRes != null && yRes != null)
            {
                list.Add(new DiscreteResolution(int.Parse(xRes.Value), int.Parse(yRes.Value)));
            }
        }
        return list;
    }

    private static List<T> ParseEnumValues<T>(IEnumerable<XElement>? elements) where T : struct
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

    private static EsclInputCaps? ParseInputCaps(XElement? element,
        Dictionary<string, EsclSettingProfile> settingProfilesMap)
    {
        if (element == null)
        {
            return null;
        }
        var settingProfiles = new List<EsclSettingProfile>();
        var settingProfilesEl = element.Element(ScanNs + "SettingProfiles");
        if (settingProfilesEl != null)
        {
            foreach (var el in settingProfilesEl.Elements(ScanNs + "SettingProfile"))
            {
                settingProfiles.Add(ParseSettingProfile(el, settingProfilesMap));
            }
        }
        return new EsclInputCaps
        {
            SettingProfiles = settingProfiles,
            MinWidth = ParseHelper.MaybeParseInt(element.Element(ScanNs + "MinWidth")),
            MaxWidth = ParseHelper.MaybeParseInt(element.Element(ScanNs + "MaxWidth")),
            MinHeight = ParseHelper.MaybeParseInt(element.Element(ScanNs + "MinHeight")),
            MaxHeight = ParseHelper.MaybeParseInt(element.Element(ScanNs + "MaxHeight")),
        };
    }
}