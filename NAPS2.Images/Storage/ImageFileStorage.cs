namespace NAPS2.Images.Storage;

public class ImageFileStorage : IImageStorage
{
    private bool _disposed;

    public ImageFileStorage(string fullPath) : this(fullPath, false)
    {
    }

    public ImageFileStorage(string fullPath, bool shared)
    {
        if (!File.Exists(fullPath ?? throw new ArgumentNullException()))
        {
            throw new FileNotFoundException(null, fullPath);
        }
        FullPath = fullPath;
        IsShared = shared;
    }

    public string FullPath { get; }

    internal bool IsDisposed => _disposed;

    internal bool IsShared { get; set; }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (IsShared) return;
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