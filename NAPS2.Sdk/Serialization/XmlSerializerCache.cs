using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using NAPS2.Util;

namespace NAPS2.Serialization
{
    public static class XmlSerializerCache
    {
        private static readonly Dictionary<string, XmlSerializer> SerializerCache = new Dictionary<string, XmlSerializer>();

        public static XmlSerializer GetSerializer(Type t, Type[] knownTypes)
        {
            knownTypes = knownTypes ?? new Type[0];
            var key = string.Join(",", knownTypes.OrderBy(x => x.FullName).Prepend(t));
            return SerializerCache.GetOrSet(key, () => new XmlSerializer(t, knownTypes));
        }
    }
}
