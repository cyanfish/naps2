﻿namespace NAPS2.Util;

internal static class FileSystemHelper
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

    // https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
    public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}