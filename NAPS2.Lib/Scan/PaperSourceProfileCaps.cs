using NAPS2.Serialization;

namespace NAPS2.Scan;

public class PaperSourceProfileCaps
{
    static PaperSourceProfileCaps()
    {
        XmlSerializer.RegisterCustomSerializer(new Serializer());
    }

    public List<ScanSource>? Values { get; set; }

    private class Serializer : CustomXmlSerializer<PaperSourceProfileCaps>
    {
        protected override void Serialize(PaperSourceProfileCaps obj, XElement element)
        {
            if (obj.Values != null)
            {
                element.Value = string.Join(",", obj.Values.Select(x => x.ToString()));
            }
        }

        protected override PaperSourceProfileCaps Deserialize(XElement element)
        {
            var caps = new PaperSourceProfileCaps();
            if (!string.IsNullOrWhiteSpace(element.Value))
            {
                caps.Values = element.Value.Split(',')
                    .Select(x => (ScanSource) Enum.Parse(typeof(ScanSource), x)).ToList();
            }
            return caps;
        }
    }
}