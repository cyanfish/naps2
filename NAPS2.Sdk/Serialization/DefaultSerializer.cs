using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            this.knownTypes = knownTypes?.ToArray();
        }

        public void Serialize(Stream stream, T obj)
        {
            var xmlSerializer = XmlSerializerCache.GetSerializer(typeof(T), knownTypes);
            xmlSerializer.Serialize(stream, obj);
        }

        public T Deserialize(Stream stream)
        {
            var xmlSerializer = XmlSerializerCache.GetSerializer(typeof(T), knownTypes);
            return (T)xmlSerializer.Deserialize(stream);
        }
    }
}
