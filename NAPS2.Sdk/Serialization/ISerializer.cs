using System.IO;

namespace NAPS2.Serialization
{
    public interface ISerializer<T>
    {
        void Serialize(Stream stream, T obj);

        T Deserialize(Stream stream);
    }
}