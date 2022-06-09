namespace NAPS2.Images.Storage;

/// <summary>
/// Base type for image storage, which can be a normal in-memory image (see IMemoryImage) or an image stored on the
/// filesystem (see ImageFileStorage).
/// </summary>
public interface IImageStorage : IDisposable
{
}