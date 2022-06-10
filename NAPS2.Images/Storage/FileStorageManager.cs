using System.Globalization;

namespace NAPS2.Images.Storage;

public class FileStorageManager : IDisposable
{
    private readonly string _prefix = Path.GetRandomFileName();
    private int _fileNumber;

    public static FileStorageManager CreateFolder(string folderPath)
    {
        new DirectoryInfo(folderPath).Create();
        return new FileStorageManager(folderPath);
    }

    public FileStorageManager(string folderPath)
    {
        FolderPath = folderPath;
    }

    public string FolderPath { get; }
        
    public virtual string NextFilePath()
    {
        lock (this)
        {
            string fileName = $"{_prefix}.{(++_fileNumber).ToString("D5", CultureInfo.InvariantCulture)}";
            return Path.Combine(FolderPath, fileName);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}