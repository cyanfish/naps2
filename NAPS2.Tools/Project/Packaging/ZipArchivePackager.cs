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

        var portableExe = Path.Combine(Paths.SolutionRoot, "NAPS2.App.PortableLauncher", "bin", "Release", "net462",
            "NAPS2.Portable.exe");
        if (!File.Exists(portableExe))
        {
            throw new Exception($"Could not find portable exe: {portableExe}");
        }
        
        using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        foreach (var file in pkgInfo.Files)
        {
            archive.CreateEntryFromFile(file.SourcePath, Path.Combine("App", file.DestPath));
        }
        archive.CreateEntry("Data/");
        archive.CreateEntryFromFile(portableExe, "NAPS2.Portable.exe");
    }
}