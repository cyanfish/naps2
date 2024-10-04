using System.IO.Compression;

namespace NAPS2.Tools.Project.Packaging;

public static class ZipArchivePackager
{
    public static void PackageZip(PackageInfo pkgInfo, bool noSign)
    {
        if (!noSign)
        {
            Output.Verbose("Signing contents");
            WindowsSigning.SignContents(pkgInfo);
        }

        var zipPath = pkgInfo.GetPath("zip");
        Output.Info($"Packaging zip archive: {zipPath}");
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
            Output.Verbose($"Compressing {destPath}");
            archive.CreateEntryFromFile(file.SourcePath, destPath);
        }
        Output.Verbose("Creating Data/");
        archive.CreateEntry("Data/");
        Output.Verbose("Compressing NAPS2.Portable.exe");
        archive.CreateEntryFromFile(portableExe, "NAPS2.Portable.exe");
        
        Output.OperationEnd($"Packaged zip archive: {zipPath}");
    }
}