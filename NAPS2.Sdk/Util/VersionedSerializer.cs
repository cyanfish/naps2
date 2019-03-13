using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace NAPS2.Util
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
            var rootName = doc.Root?.Name.ToString();
            var versionElement = doc.Root?.Descendants("Version").FirstOrDefault();
            int.TryParse(versionElement?.Value, out var version);
            return InternalDeserialize(stream, rootName, version);
        }

        protected abstract void InternalSerialize(Stream stream, T obj);

        protected abstract T InternalDeserialize(Stream stream, string rootName, int version);

        protected abstract IEnumerable<Type> KnownTypes { get; }

        protected void XmlSerialize(Stream stream, T obj)
        {
            var xmlSerializer = new XmlSerializer(typeof(T), KnownTypesArray);
            xmlSerializer.Serialize(stream, obj);
        }

        protected T XmlDeserialize(Stream stream)
        {
            var xmlSerializer = new XmlSerializer(typeof(T), KnownTypesArray);
            return (T)xmlSerializer.Deserialize(stream);
        }

        protected T2 XmlDeserialize<T2>(Stream stream)
        {
            var xmlSerializer = new XmlSerializer(typeof(T2), KnownTypesArray);
            return (T2)xmlSerializer.Deserialize(stream);
        }

        private Type[] KnownTypesArray => KnownTypes?.ToArray() ?? new Type[] { };
    }
}
