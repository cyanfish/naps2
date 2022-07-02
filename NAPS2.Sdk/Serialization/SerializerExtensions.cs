using System.Text;

namespace NAPS2.Serialization;

public static class SerializerExtensions
{
    public static void SerializeToFile<T>(this ISerializer<T> serializer, string path, T obj) where T : notnull
    {
        using var stream = File.Open(path, FileMode.Create);
        serializer.Serialize(stream, obj);
    }

    public static byte[] SerializeToBytes<T>(this ISerializer<T> serializer, T obj) where T : notnull
    {
        using var stream = new MemoryStream();
        serializer.Serialize(stream, obj);
        return stream.ToArray();
    }

    public static string SerializeToString<T>(this ISerializer<T> serializer, T obj) where T : notnull
    {
        return Encoding.UTF8.GetString(serializer.SerializeToBytes(obj));
    }

    public static T DeserializeFromFile<T>(this ISerializer<T> serializer, string path) where T : notnull
    {
        using var stream = File.Open(path, FileMode.Open);
        return serializer.Deserialize(stream) ?? throw new InvalidOperationException("Deserialized to null");
    }

    public static T DeserializeFromBytes<T>(this ISerializer<T> serializer, byte[] bytes) where T : notnull
    {
        using var stream = new MemoryStream(bytes);
        return serializer.Deserialize(stream) ?? throw new InvalidOperationException("Deserialized to null");
    }

    public static T DeserializeFromString<T>(this ISerializer<T> serializer, string str) where T : notnull
    {
        return serializer.DeserializeFromBytes(Encoding.UTF8.GetBytes(str));
    }

    public static string ToXml<T>(this T obj) where T : notnull => new XmlSerializer<T>().SerializeToString(obj);

    public static T FromXml<T>(this string xml) where T : notnull => new XmlSerializer<T>().DeserializeFromString(xml);
}