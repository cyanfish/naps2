namespace NAPS2.Images.Storage;

public class ImageFileStorage : IImageStorage
{
    private readonly bool _shared;

    public ImageFileStorage(string fullPath) : this(fullPath, false)
    {
    }

    public ImageFileStorage(string fullPath, bool shared)
    {
        FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
        _shared = shared;
    }

    public string FullPath { get; }

    public void Dispose()
    {
        if (!_shared)
        {
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
}