using System;

namespace NAPS2.Images.Storage;

public interface IImage : IStorage
{
    int Width { get; }

    int Height { get; }

    float HorizontalResolution { get; }

    float VerticalResolution { get; }

    void SetResolution(float xDpi, float yDpi);

    StoragePixelFormat PixelFormat { get; }

    bool IsOriginalLossless { get; }

    object Lock(LockMode lockMode, out IntPtr scan0, out int stride);

    void Unlock(object state);

    IImage Clone();
}