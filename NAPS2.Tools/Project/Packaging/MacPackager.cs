using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Packaging;

public static class MacPackager
{
    public static void Package(PackageInfo packageInfo, bool verbose)
    {
        var pkgPath = packageInfo.GetPath("pkg");
        Console.WriteLine($"Packaging installer: {pkgPath}");

        var runtimeId = packageInfo.Platform == Platform.MacArm ? "osx-arm64" : "osx-x64";
        Cli.Run("dotnet", $"publish NAPS2.App.Mac -c InstallerEXE -r {runtimeId}", verbose);
        // TODO: Fix version
        var sourcePath = Path.Combine(Paths.SolutionRoot, "NAPS2.App.Mac", "bin", "InstallerEXE", "net6-macos10.15",
            runtimeId, "publish", "NAPS2-1.0.pkg");
        if (File.Exists(pkgPath))
        {
            File.Delete(pkgPath);
        }
        File.Copy(sourcePath, pkgPath);

        Console.WriteLine(verbose ? $"Packaged installer: {pkgPath}" : "Done.");
    }
}