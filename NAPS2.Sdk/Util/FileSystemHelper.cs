namespace NAPS2.Util;

public static class FileSystemHelper
{
    /// <summary>
    /// Creates the parent directory for the provided path if needed.
    /// </summary>
    /// <param name="filePath"></param>
    public static void EnsureParentDirExists(string filePath)
    {
        var parentDir = new FileInfo(filePath).Directory;
        if (parentDir != null && !parentDir.Exists)
        {
            parentDir.Create();
        }
    }

    public static bool IsFileInUse(string filePath, out Exception? exception)
    {
        if (File.Exists(filePath))
        {
            try
            {
                using (new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                }
            }
            catch (IOException ex)
            {
                exception = ex;
                return true;
            }
        }
        exception = null;
        return false;
    }
}