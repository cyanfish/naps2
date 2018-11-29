using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Images.Storage
{
    public class MemoryStreamStorage : IStorage
    {
        public MemoryStreamStorage(MemoryStream stream)
        {
            Stream = stream;
        }

        public MemoryStream Stream { get; }

        public void Dispose()
        {
            Stream.Dispose();
        }
    }
}
