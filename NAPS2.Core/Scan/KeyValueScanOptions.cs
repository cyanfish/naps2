using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace NAPS2.Scan
{
    public class KeyValueScanOptions : Dictionary<string, string>, IXmlSerializable
    {
        public XmlSchema GetSchema()
        {
            return null;
        }

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
