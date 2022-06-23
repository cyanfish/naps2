using NAPS2.Serialization;

namespace NAPS2.Config;

public class ConfigStorageSerializer<TConfig> : ISerializer<ConfigStorage<TConfig>>
{
    public void Serialize(Stream stream, ConfigStorage<TConfig>? obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        var doc = new XDocument();
        obj.SerializeTo(doc);
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