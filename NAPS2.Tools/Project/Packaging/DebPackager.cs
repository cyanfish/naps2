using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Packaging;

public static class DebPackager
{
    public static void PackageDeb(PackageInfo pkgInfo)
    {
        var debPath = pkgInfo.GetPath("deb");
        Output.Info($"Packaging deb: {debPath}");

        Output.Verbose("Building binaries");
        var runtimeId = pkgInfo.Platform == Platform.LinuxArm ? "linux-arm64" : "linux-x64";
        Cli.Run("dotnet", $"clean NAPS2.App.Gtk -c Release -r {runtimeId}");
        Cli.Run("dotnet", $"publish NAPS2.App.Gtk -c Release -r {runtimeId} --self-contained /p:DebugType=None /p:DebugSymbols=false");

        Output.Verbose("Creating package");

        var workingDir = Path.Combine(Paths.SetupObj, "deb");
        if (Directory.Exists(workingDir))
        {
            Directory.Delete(workingDir, true);
        }

        Directory.CreateDirectory(workingDir);

        var controlDir = Path.Combine(workingDir, "DEBIAN");
        Directory.CreateDirectory(controlDir);

        // Create control files
        var template = File.ReadAllText(Path.Combine(Paths.SetupLinux, "debian-control"));
        template = template.Replace("{!arch}", pkgInfo.Platform == Platform.LinuxArm ? "arm64" : "amd64");
        template = template.Replace("{!version}", pkgInfo.VersionNumber);
        File.WriteAllText(Path.Combine(controlDir, "control"), template);

        // Copy binary files
        var publishDir = Path.Combine(Paths.SolutionRoot, "NAPS2.App.Gtk", "bin", "Release", "net8", runtimeId,
            "publish");
        var targetDir = Path.Combine(workingDir, "usr/lib/naps2");
        ProjectHelper.CopyDirectory(publishDir, targetDir);

        // Copy metadata files
        var iconDir = Path.Combine(workingDir, "usr/share/icons/hicolor/128x128/apps");
        Directory.CreateDirectory(iconDir);
        var appsDir = Path.Combine(workingDir, "usr/share/applications");
        Directory.CreateDirectory(appsDir);
        var metainfoDir = Path.Combine(workingDir, "usr/share/metainfo");
        Directory.CreateDirectory(metainfoDir);
        File.Copy(
            Path.Combine(Paths.SolutionRoot, "NAPS2.Lib", "Icons", "scanner-128.png"),
            Path.Combine(iconDir, "com.naps2.Naps2.png"));
        File.Copy(
            Path.Combine(Paths.SetupLinux, "com.naps2.Naps2.desktop"),
            Path.Combine(appsDir, "naps2.desktop"));
        File.WriteAllText(Path.Combine(metainfoDir, "com.naps2.Naps2.metainfo.xml"),
            ProjectHelper.GetLinuxMetaInfo(pkgInfo));
        File.Copy(
            Path.Combine(Paths.SolutionRoot, "LICENSE"),
            Path.Combine(targetDir, "LICENSE.txt"));

        // Create symlinks
        var binDir = Path.Combine(workingDir, "usr/bin");
        Directory.CreateDirectory(binDir);
        Cli.Run("ln", $"-s /usr/lib/naps2/naps2 {Path.Combine(binDir, "naps2")}");

        // Fix permissions
        var nativeLibsFolder = pkgInfo.Platform == Platform.LinuxArm ? "_linuxarm" : "_linux";
        Cli.Run("chmod", $"a+x {Path.Combine(targetDir, nativeLibsFolder, "tesseract")}");

        Cli.Run("dpkg-deb", $"-Zxz --root-owner-group --build {workingDir} {debPath}");

        Output.OperationEnd($"Packaged deb: {debPath}");
    }
}