using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        public static string SerializeToString<T>(this ISerializer<T> serializer, T obj)
        {
            return Encoding.UTF8.GetString(serializer.SerializeToBytes(obj));
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

        public static T DeserializeFromString<T>(this ISerializer<T> serializer, string str)
        {
            return serializer.DeserializeFromBytes(Encoding.UTF8.GetBytes(str));
        }

        public static string ToXml<T>(this T obj) => obj.ToXml(null);

        public static string ToXml<T>(this T obj, IEnumerable<Type> knownTypes)
        {
            return new DefaultSerializer<T>(knownTypes).SerializeToString(obj);
        }

        public static T FromXml<T>(this string xml) => xml.FromXml<T>(null);

        public static T FromXml<T>(this string xml, IEnumerable<Type> knownTypes)
        {
            return new DefaultSerializer<T>(knownTypes).DeserializeFromString(xml);
        }
    }
}
