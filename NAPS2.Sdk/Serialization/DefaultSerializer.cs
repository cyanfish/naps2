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
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<string, XmlSerializer> SerializerCache = new Dictionary<string, XmlSerializer>();

        private static XmlSerializer GetSerializer(Type[] knownTypes)
        {
            var key = knownTypes == null ? "" : string.Join(",", knownTypes.OrderBy(x => x.FullName));
            return SerializerCache.GetOrSet(key, () => new XmlSerializer(typeof(T), knownTypes ?? new Type[0]));
        }

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
            GetSerializer(knownTypes).Serialize(stream, obj);
        }

        public T Deserialize(Stream stream)
        {
            return (T)GetSerializer(knownTypes).Deserialize(stream);
        }
    }
}
