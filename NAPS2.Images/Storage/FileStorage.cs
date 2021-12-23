namespace NAPS2.Images.Storage;

public class FileStorage : IStorage
{
    private readonly bool _shared;

    public FileStorage(string fullPath) : this(fullPath, false)
    {
    }

    public FileStorage(string fullPath, bool shared)
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
            }
        }
    }
}