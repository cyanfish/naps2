using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;

namespace NAPS2.Scan.Images.Storage
{
    // TODO: Maybe just call this IImage.
    public interface IMemoryStorage : IStorage
    {
        int Width { get; }

        int Height { get; }

        float HorizontalResolution { get; }

        float VerticalResolution { get; }

        void SetResolution(float xDpi, float yDpi);

        StoragePixelFormat PixelFormat { get; }

        bool IsOriginalLossless { get; }

        object Lock(out IntPtr scan0, out int stride);

        void Unlock(object state);

        IMemoryStorage Clone();
    }
}
