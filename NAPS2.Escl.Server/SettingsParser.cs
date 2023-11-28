using System.Globalization;
using System.Xml.Linq;

namespace NAPS2.Escl.Server;

internal static class SettingsParser
{
    private static readonly XNamespace ScanNs = EsclXmlHelper.ScanNs;
    private static readonly XNamespace PwgNs = EsclXmlHelper.PwgNs;

    public static EsclScanSettings Parse(XDocument doc)
    {
        var root = doc.Root;
        if (root?.Name != ScanNs + "ScanSettings")
        {
            throw new InvalidOperationException("Unexpected root element: " + doc.Root?.Name);
        }
        var scanRegion = root!.Element(PwgNs + "ScanRegions")?.Elements(PwgNs + "ScanRegion").FirstOrDefault();
        return new EsclScanSettings
        {
            // TODO: Handle intents?
            InputSource = MaybeParseEnum(root.Element(PwgNs + "InputSource"), EsclInputSource.Platen),
            ColorMode = MaybeParseEnum(root.Element(ScanNs + "ColorMode"), EsclColorMode.RGB24),
            DocumentFormat = root.Element(ScanNs + "DocumentFormat")?.Value,
            Duplex = root.Element(ScanNs + "Duplex")?.Value == "true",
            XResolution = MaybeParseInt(root.Element(ScanNs + "XResolution")) ?? 0,
            YResolution = MaybeParseInt(root.Element(ScanNs + "YResolution")) ?? 0,
            Width = MaybeParseInt(scanRegion?.Element(PwgNs + "Width")) ?? 0,
            Height = MaybeParseInt(scanRegion?.Element(PwgNs + "Height")) ?? 0,
            XOffset = MaybeParseInt(scanRegion?.Element(PwgNs + "XOffset")) ?? 0,
            YOffset = MaybeParseInt(scanRegion?.Element(PwgNs + "YOffset")) ?? 0,
        };
    }

    private static T MaybeParseEnum<T>(XElement? element, T defaultValue) where T : struct =>
        Enum.TryParse<T>(element?.Value, out var value) ? value : defaultValue;

    private static int? MaybeParseInt(XElement? element) =>
        int.TryParse(element?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : null;
}