using System.Xml.Linq;

namespace NAPS2.Escl.Server;

internal static class SettingsParser
{
    private static readonly XNamespace ScanNs = EsclXmlHelper.ScanNs;
    private static readonly XNamespace PwgNs = EsclXmlHelper.PwgNs;

    // The eSCL specification writes the date part of the scan namespace with hyphens, e.g.
    // "http://schemas.hp.com/imaging/escl/2011-05-03", which is what other implementations and
    // hardware clients send. NAPS2 has always used slashes, so both are accepted here - otherwise
    // standard-compliant clients would be rejected with a 400. This mirrors how the client-side
    // CapabilitiesParser matches namespaces.
    private static readonly XNamespace[] ScanNsAll =
    [
        ScanNs, XNamespace.Get("http://schemas.hp.com/imaging/escl/2011-05-03")
    ];

    public static EsclScanSettings Parse(XDocument doc)
    {
        var root = doc.Root;
        if (root?.Name.LocalName != "ScanSettings" || !ScanNsAll.Contains(root.Name.Namespace))
        {
            throw new InvalidOperationException("Unexpected root element: " + doc.Root?.Name);
        }
        var scanRegion = root!.Element(PwgNs + "ScanRegions")?.Elements(PwgNs + "ScanRegion").FirstOrDefault();
        return new EsclScanSettings
        {
            // TODO: Handle intents?
            InputSource = ParseHelper.MaybeParseEnum(root.Element(PwgNs + "InputSource"), EsclInputSource.Platen),
            ColorMode = ParseHelper.MaybeParseEnum(ScanElement(root, "ColorMode"), EsclColorMode.RGB24),
            DocumentFormat = ScanElement(root, "DocumentFormatExt")?.Value ??
                             root.Element(PwgNs + "DocumentFormat")?.Value,
            Duplex = ScanElement(root, "Duplex")?.Value == "true",
            XResolution = ParseHelper.MaybeParseInt(ScanElement(root, "XResolution")) ?? 0,
            YResolution = ParseHelper.MaybeParseInt(ScanElement(root, "YResolution")) ?? 0,
            Width = ParseHelper.MaybeParseInt(scanRegion?.Element(PwgNs + "Width")) ?? 0,
            Height = ParseHelper.MaybeParseInt(scanRegion?.Element(PwgNs + "Height")) ?? 0,
            XOffset = ParseHelper.MaybeParseInt(scanRegion?.Element(PwgNs + "XOffset")) ?? 0,
            YOffset = ParseHelper.MaybeParseInt(scanRegion?.Element(PwgNs + "YOffset")) ?? 0,
            CompressionFactor = ParseHelper.MaybeParseInt(ScanElement(root, "CompressionFactor"))
        };
    }

    private static XElement? ScanElement(XElement root, string localName) =>
        root.Elements().FirstOrDefault(e => ScanNsAll.Contains(e.Name.Namespace) && e.Name.LocalName == localName);
}
