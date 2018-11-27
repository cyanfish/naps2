using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Scan.Images.Storage
{
    public interface IMemoryStorageFactory
    {
        IStorage FromBmpStream(Stream stream);

        IStorage FromDimensions(int width, int height, StoragePixelFormat pixelFormat);
    }
}
