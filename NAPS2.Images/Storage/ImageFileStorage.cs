namespace NAPS2.Images.Storage;

public class ImageFileStorage : IImageStorage
{
    private bool _shared;
    private bool _disposed;

    public ImageFileStorage(string fullPath) : this(fullPath, false)
    {
    }

    public ImageFileStorage(string fullPath, bool shared)
    {
        FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
        _shared = shared;
    }

    public string FullPath { get; }

    internal bool IsDisposed => _disposed;

    internal void MarkShared()
    {
        _shared = true;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_shared) return;
        try
        {
            File.Delete(FullPath);
        }
        catch (IOException)
        {
            // TODO: Log this
        }
    }
}