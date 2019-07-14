using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NAPS2.Serialization
{
    public abstract class VersionedSerializer<T> : ISerializer<T>
    {
        public void Serialize(Stream stream, T obj)
        {
            InternalSerialize(stream, obj);
        }

        public T Deserialize(Stream stream)
        {
            var doc = XDocument.Load(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return InternalDeserialize(stream, doc);
        }

        protected abstract void InternalSerialize(Stream stream, T obj);

        protected abstract T InternalDeserialize(Stream stream, XDocument doc);

        protected void XmlSerialize(Stream stream, T obj)
        {
            var xmlSerializer = new XmlSerializer<T>();
            xmlSerializer.Serialize(stream, obj);
        }

        protected T XmlDeserialize(Stream stream)
        {
            var xmlSerializer = new XmlSerializer<T>();
            return xmlSerializer.Deserialize(stream);
        }

        protected T2 XmlDeserialize<T2>(Stream stream)
        {
            var xmlSerializer = new XmlSerializer<T2>();
            return xmlSerializer.Deserialize(stream);
        }

        protected int GetVersion(XDocument doc)
        {
            var versionElement = doc.Root?.Descendants("Version").FirstOrDefault();
            int.TryParse(versionElement?.Value, out var version);
            return version;
        }
    }
}
