using System.IO.Compression;
using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Packaging;

public static class ZipArchivePackager
{
    public static void PackageZip(Func<PackageInfo> pkgInfoFunc, Platform platform, bool noSign)
    {
        string arch = platform == Platform.WinArm64 ? "arm64" : "x64";

        Output.Verbose("Building binaries");
        if (platform != Platform.WinArm64)
        {
            Cli.Run("dotnet", $"clean NAPS2.App.Worker -r win-{arch} -c Release");
        }
        Cli.Run("dotnet", $"clean NAPS2.App.WinForms -r win-{arch} -c Release");
        Cli.Run("dotnet", $"clean NAPS2.App.Console -r win-{arch} -c Release");
        if (platform != Platform.WinArm64)
        {
            Cli.Run("dotnet",
                $"publish NAPS2.App.Worker -r win-{arch} -c Release /p:DebugType=None /p:DebugSymbols=false /p:DefineConstants=ZIP");
        }
        Cli.Run("dotnet", $"publish NAPS2.App.WinForm -r win-{arch}s -c Release /p:DebugType=None /p:DebugSymbols=false /p:DefineConstants=ZIP");
        Cli.Run("dotnet", $"publish NAPS2.App.Console -r win-{arch} -c Release /p:DebugType=None /p:DebugSymbols=false /p:DefineConstants=ZIP");
        Cli.Run("dotnet", "build NAPS2.App.PortableLauncher -c Release");

        var pkgInfo = pkgInfoFunc();
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
        if (!noSign)
        {
            Output.Verbose("Signing NAPS2.Portable.exe");
            WindowsSigning.SignFile(portableExe);
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