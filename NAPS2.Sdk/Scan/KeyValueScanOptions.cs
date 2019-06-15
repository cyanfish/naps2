using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NAPS2.Serialization;

namespace NAPS2.Scan
{
    /// <summary>
    /// A set of key-value options used for scanning.
    ///
    /// This is only relevant for SANE. Currently NAPS2 does not actually support viewing/setting custom options.
    /// If someone was so inclined they could manually set them in the profiles.xml file.
    /// </summary>
    public class KeyValueScanOptions : Dictionary<string, string>
    {
        public KeyValueScanOptions()
        {
        }

        public KeyValueScanOptions(IDictionary<string, string> dictionary) : base(dictionary)
        {
        }

        // ReSharper disable once UnusedMember.Local
        private class Serializer : CustomXmlSerializer<KeyValueScanOptions>
        {
            protected override void Serialize(KeyValueScanOptions obj, XElement element)
            {
                foreach (var kvp in obj)
                {
                    var itemElement = new XElement("Option", kvp.Value);
                    itemElement.SetAttributeValue("name", kvp.Key);
                    element.Add(itemElement);
                }
            }

            protected override KeyValueScanOptions Deserialize(XElement element)
            {
                var obj = new KeyValueScanOptions();
                foreach (var itemElement in element.Elements())
                {
                    obj.Add(itemElement.Attribute("name").Value, itemElement.Value);
                }
                return obj;
            }
        }
    }
}
