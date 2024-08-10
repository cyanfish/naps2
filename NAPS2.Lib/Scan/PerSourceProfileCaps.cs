using System.Globalization;
using NAPS2.Serialization;

namespace NAPS2.Scan;

public class PerSourceProfileCaps
{
    static PerSourceProfileCaps()
    {
        XmlSerializer.RegisterCustomSerializer(new Serializer());
    }

    public PageSize? ScanArea { get; set; }
    public List<int>? Resolutions { get; set; }

    private class Serializer : CustomXmlSerializer<PerSourceProfileCaps>
    {
        protected override void Serialize(PerSourceProfileCaps obj, XElement element)
        {
            if (obj.ScanArea != null)
            {
                element.Add(new XElement("ScanArea", obj.ScanArea));
            }
            if (obj.Resolutions != null)
            {
                element.Add(new XElement("Resolutions",
                    string.Join(",", obj.Resolutions.Select(x => x.ToString(CultureInfo.InvariantCulture)))));
            }
        }

        protected override PerSourceProfileCaps Deserialize(XElement element)
        {
            var caps = new PerSourceProfileCaps();
            if (element.Element("ScanArea") is { } scanArea)
            {
                caps.ScanArea = PageSize.Parse(scanArea.Value);
            }
            if (element.Element("Resolutions") is { } resolutions)
            {
                caps.Resolutions = resolutions.Value.Split(',')
                    .Select(x => int.Parse(x, NumberStyles.Integer, CultureInfo.InvariantCulture)).ToList();
            }
            return caps;
        }
    }
}