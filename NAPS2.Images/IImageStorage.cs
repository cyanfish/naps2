namespace NAPS2.Images;

/// <summary>
/// Base type for image storage, which can be a normal in-memory image or an image stored on the filesystem.
/// </summary>
public interface IImageStorage : IDisposable
{
}