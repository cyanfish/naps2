using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace NAPS2.Scan
{
    /// <summary>
    /// A set of key-value options used for scanning.
    ///
    /// This is only relevant for SANE. Currently NAPS2 does not actually support viewing/setting custom options.
    /// If someone was so inclined they could manually set them in the profiles.xml file.
    /// </summary>
    public class KeyValueScanOptions : Dictionary<string, string>, IXmlSerializable
    {
        public KeyValueScanOptions()
        {
        }

        public KeyValueScanOptions(IDictionary<string, string> dictionary) : base(dictionary)
        {
        }

        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            reader.Read();
            while (true)
            {
                if (reader.Name != @"Option")
                {
                    break;
                }
                reader.MoveToAttribute("name");
                string k = reader.Value;
                string v = reader.ReadElementString();
                Add(k, v);
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var kvp in this)
            {
                writer.WriteStartElement("Option");
                writer.WriteAttributeString("name", kvp.Key);
                writer.WriteString(kvp.Value);
                writer.WriteEndElement();
            }
        }
    }
}
