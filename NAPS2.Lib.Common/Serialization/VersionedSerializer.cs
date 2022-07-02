namespace NAPS2.Serialization;

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

    protected int GetVersion(XDocument doc)
    {
        var versionElement = doc.Root?.Descendants("Version").FirstOrDefault();
        int.TryParse(versionElement?.Value, out var version);
        return version;
    }
}