using System.IO;

namespace NAPS2.Images.Storage
{
    public class FileStorageManager
    {
        public FileStorageManager() : this(Paths.Temp)
        {
        }

        public FileStorageManager(string folderPath)
        {
            FolderPath = folderPath;
        }

        protected string FolderPath { get; }

        public virtual string NextFilePath() => Path.Combine(FolderPath, Path.GetRandomFileName());
    }
}
