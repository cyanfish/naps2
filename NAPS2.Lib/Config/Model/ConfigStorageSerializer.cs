using NAPS2.Serialization;

namespace NAPS2.Config.Model;

public class ConfigStorageSerializer<TConfig> : ISerializer<ConfigStorage<TConfig>>
{
    public void Serialize(Stream stream, ConfigStorage<TConfig>? obj)
    {
        Serialize(stream, obj, null);
    }

    public void Serialize(Stream stream, ConfigStorage<TConfig>? obj, string? customRootElementName)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        var doc = new XDocument();
        obj.SerializeTo(doc, customRootElementName);
        doc.Save(stream);
    }

    public ConfigStorage<TConfig> Deserialize(Stream stream)
    {
        var doc = XDocument.Load(stream);
        var storage = new ConfigStorage<TConfig>();
        storage.DeserializeFrom(doc);
        return storage;
    }
}