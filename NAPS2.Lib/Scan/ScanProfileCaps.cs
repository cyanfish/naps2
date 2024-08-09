using System.Globalization;
using NAPS2.Serialization;

namespace NAPS2.Scan;

public class ScanProfileCaps
{
    static ScanProfileCaps()
    {
        XmlSerializer.RegisterCustomSerializer(new Serializer());
    }

    public List<ScanSource>? PaperSources { get; set; }
    public bool? FeederCheck { get; set; }
    public List<int>? GlassResolutions { get; set; }
    public List<int>? FeederResolutions { get; set; }
    public List<int>? DuplexResolutions { get; set; }

    private class Serializer : CustomXmlSerializer<ScanProfileCaps>
    {
        protected override void Serialize(ScanProfileCaps obj, XElement element)
        {
            if (obj.PaperSources != null)
            {
                element.Add(new XElement("PaperSources", string.Join(",", obj.PaperSources.Select(x => x.ToString()))));
            }
            if (obj.FeederCheck != null)
            {
                element.Add(new XElement("FeederCheck", obj.FeederCheck));
            }
            if (obj.GlassResolutions != null)
            {
                element.Add(new XElement("GlassResolutions",
                    string.Join(",", obj.GlassResolutions.Select(x => x.ToString(CultureInfo.InvariantCulture)))));
            }
            if (obj.FeederResolutions != null)
            {
                element.Add(new XElement("FeederResolutions",
                    string.Join(",", obj.FeederResolutions.Select(x => x.ToString(CultureInfo.InvariantCulture)))));
            }
            if (obj.DuplexResolutions != null)
            {
                element.Add(new XElement("DuplexResolutions",
                    string.Join(",", obj.DuplexResolutions.Select(x => x.ToString(CultureInfo.InvariantCulture)))));
            }
        }

        protected override ScanProfileCaps Deserialize(XElement element)
        {
            var caps = new ScanProfileCaps();
            if (element.Element("FeederCheck") is { } feederCheck)
            {
                caps.FeederCheck = bool.Parse(feederCheck.Value);
            }
            if (element.Element("PaperSources") is { } paperSources)
            {
                caps.PaperSources = paperSources.Value.Split(',')
                    .Select(x => (ScanSource) Enum.Parse(typeof(ScanSource), x)).ToList();
            }
            if (element.Element("GlassResolutions") is { } glassResolutions)
            {
                caps.GlassResolutions = glassResolutions.Value.Split(',')
                    .Select(x => int.Parse(x, NumberStyles.Integer, CultureInfo.InvariantCulture)).ToList();
            }
            if (element.Element("FeederResolutions") is { } feederResolutions)
            {
                caps.FeederResolutions = feederResolutions.Value.Split(',')
                    .Select(x => int.Parse(x, NumberStyles.Integer, CultureInfo.InvariantCulture)).ToList();
            }
            if (element.Element("DuplexResolutions") is { } duplexResolutions)
            {
                caps.DuplexResolutions = duplexResolutions.Value.Split(',')
                    .Select(x => int.Parse(x, NumberStyles.Integer, CultureInfo.InvariantCulture)).ToList();
            }
            return caps;
        }
    }
}