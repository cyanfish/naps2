using System.IO;

namespace NAPS2.Util
{
    public static class PathHelper
    {
        public static void EnsureParentDirExists(string filePath)
        {
            var parentDir = new FileInfo(filePath).Directory;
            if (parentDir?.Exists == false)
            {
                parentDir.Create();
            }
        }
    }
}