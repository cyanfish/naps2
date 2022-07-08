using System.IO.Compression;

namespace NAPS2.Tools.Project.Packaging;

public static class ZipArchivePackager
{
    public static void PackageZip(PackageInfo pkgInfo)
    {
        var zipPath = Path.Combine(Paths.Publish, pkgInfo.Version, $"{pkgInfo.FileName}.zip");
        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }
        using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        foreach (var file in pkgInfo.Files)
        {
            archive.CreateEntryFromFile(file.SourcePath, file.DestPath);
        }
    }
}