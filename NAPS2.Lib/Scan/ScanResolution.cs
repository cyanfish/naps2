using System.Globalization;
using NAPS2.Serialization;

namespace NAPS2.Scan;

public class ScanResolution
{
    static ScanResolution()
    {
        XmlSerializer.RegisterCustomSerializer(new Serializer());
    }

    public int Dpi { get; set; }

    // For backwards-compatibility reasons, we serialize this as "Dpi100" instead of just serializing an integer.
    private class Serializer : CustomXmlSerializer<ScanResolution>
    {
        protected override void Serialize(ScanResolution obj, XElement element)
        {
            element.Value = $"Dpi{obj.Dpi.ToString(CultureInfo.InvariantCulture)}";
        }

        protected override ScanResolution Deserialize(XElement element)
        {
            var value = element.Value;
            if (value.StartsWith("Dpi") && int.TryParse(value.Substring(3), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out int dpi))
            {
                return new ScanResolution { Dpi = dpi };
            }
            return new ScanResolution();
        }
    }
}