using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Serialization
{
    public interface ISerializer<T>
    {
        void Serialize(Stream stream, T obj);

        T Deserialize(Stream stream);
    }
}