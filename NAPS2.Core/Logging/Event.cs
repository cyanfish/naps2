using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NAPS2.Logging
{
    public class Event
    {
        private static readonly Lazy<XmlSerializer> Serializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(Event)));

        public string Name { get; set; }

        public int? Pages { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string User { get; set; } = $"{Environment.UserDomainName}\\{Environment.UserName}";

        public string DeviceName { get; set; }

        public string ProfileName { get; set; }

        public string BitDepth { get; set; }

        public string FileFormat { get; set; }

        public override string ToString()
        {
            var stream = new MemoryStream();
            Serializer.Value.Serialize(stream, this);
            stream.Seek(0, SeekOrigin.Begin);
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}