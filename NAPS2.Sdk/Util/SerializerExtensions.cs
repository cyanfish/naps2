using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Util
{
    public static class SerializerExtensions
    {
        public static void SerializeToFile<T>(this ISerializer<T> serializer, string path, T obj)
        {
            using (var stream = File.Open(path, FileMode.Create))
            {
                serializer.Serialize(stream, obj);
            }
        }

        public static byte[] SerializeToBytes<T>(this ISerializer<T> serializer, T obj)
        {
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        public static T DeserializeFromFile<T>(this ISerializer<T> serializer, string path)
        {
            using (var stream = File.Open(path, FileMode.Open))
            {
                return serializer.Deserialize(stream);
            }
        }

        public static T DeserializeFromBytes<T>(this ISerializer<T> serializer, byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return serializer.Deserialize(stream);
            }
        }
    }
}
