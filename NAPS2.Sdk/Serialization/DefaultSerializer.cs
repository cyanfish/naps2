using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using NAPS2.Util;

namespace NAPS2.Serialization
{
    public class DefaultSerializer<T> : ISerializer<T>
    {
        private readonly Type[] knownTypes;

        public DefaultSerializer()
        {
        }

        public DefaultSerializer(IEnumerable<Type> knownTypes)
        {
            this.knownTypes = this.knownTypes?.ToArray();
        }

        public void Serialize(Stream stream, T obj)
        {
            var xmlSerializer = new XmlSerializer(typeof(T), knownTypes);
            xmlSerializer.Serialize(stream, obj);
        }

        public T Deserialize(Stream stream)
        {
            var xmlSerializer = new XmlSerializer(typeof(T), knownTypes);
            return (T)xmlSerializer.Deserialize(stream);
        }
    }
}
