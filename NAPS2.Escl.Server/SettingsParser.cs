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
            InputSource = ParseHelper.MaybeParseEnum(root.Element(PwgNs + "InputSource"), EsclInputSource.Platen),
            ColorMode = ParseHelper.MaybeParseEnum(root.Element(ScanNs + "ColorMode"), EsclColorMode.RGB24),
            DocumentFormat = root.Element(ScanNs + "DocumentFormatExt")?.Value ??
                             root.Element(PwgNs + "DocumentFormat")?.Value,
            Duplex = root.Element(ScanNs + "Duplex")?.Value == "true",
            XResolution = ParseHelper.MaybeParseInt(root.Element(ScanNs + "XResolution")) ?? 0,
            YResolution = ParseHelper.MaybeParseInt(root.Element(ScanNs + "YResolution")) ?? 0,
            Width = ParseHelper.MaybeParseInt(scanRegion?.Element(PwgNs + "Width")) ?? 0,
            Height = ParseHelper.MaybeParseInt(scanRegion?.Element(PwgNs + "Height")) ?? 0,
            XOffset = ParseHelper.MaybeParseInt(scanRegion?.Element(PwgNs + "XOffset")) ?? 0,
            YOffset = ParseHelper.MaybeParseInt(scanRegion?.Element(PwgNs + "YOffset")) ?? 0,
            CompressionFactor = ParseHelper.MaybeParseInt(root.Element(ScanNs + "CompressionFactor"))
        };
    }
}