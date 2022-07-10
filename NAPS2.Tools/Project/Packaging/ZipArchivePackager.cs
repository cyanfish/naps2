using System.IO.Compression;

namespace NAPS2.Tools.Project.Packaging;

public static class ZipArchivePackager
{
    public static void PackageZip(PackageInfo pkgInfo, bool verbose)
    {
        var zipPath = pkgInfo.GetPath("zip");
        Console.WriteLine($"Packaging zip archive: {zipPath}");
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
            var destPath = Path.Combine("App", file.DestPath);
            if (verbose)
            {
                Console.WriteLine($"Compressing {destPath}");
            }
            archive.CreateEntryFromFile(file.SourcePath, destPath);
        }
        if (verbose)
        {
            Console.WriteLine($"Creating Data/");
        }
        archive.CreateEntry("Data/");
        if (verbose)
        {
            Console.WriteLine($"Compressing NAPS2.Portable.exe");
        }
        archive.CreateEntryFromFile(portableExe, "NAPS2.Portable.exe");
        
        Console.WriteLine(verbose ? $"Packaged zip archive: {zipPath}" : "Done.");
    }
}