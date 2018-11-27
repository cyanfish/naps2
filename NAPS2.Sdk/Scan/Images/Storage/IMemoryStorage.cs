using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Images.Storage
{
    public interface IMemoryStorage : IStorage
    {
        int Width { get; }

        int Height { get; }

        StoragePixelFormat PixelFormat { get; }

        object Lock(out IntPtr scan0, out int stride);

        void Unlock(object state);
    }
}
