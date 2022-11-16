using System.Text;
using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Packaging;

public static class MacPackager
{
    private const string UNIVERSAL_BUNDLE_X64_LIBS = "libs---x64";
    private const string UNIVERSAL_BUNDLE_ARM64_LIBS = "libs-arm64";

    public static void Package(PackageInfo packageInfo)
    {
        var pkgPath = packageInfo.GetPath("pkg");
        Output.Info($"Packaging installer: {pkgPath}");

        if (packageInfo.Platform == Platform.Mac)
        {
            BuildUniversalAppBundle(pkgPath);
        }
        else
        {
            var runtimeId = packageInfo.Platform == Platform.MacArm ? "osx-arm64" : "osx-x64";
            Cli.Run("dotnet", $"publish NAPS2.App.Mac -c InstallerEXE -r {runtimeId}");
            // TODO: Fix version
            var sourcePath = Path.Combine(Paths.SolutionRoot, "NAPS2.App.Mac", "bin", "InstallerEXE", "net7-macos10.15",
                runtimeId, "publish", "NAPS2-1.0.pkg");
            if (File.Exists(pkgPath))
            {
                File.Delete(pkgPath);
            }
            File.Copy(sourcePath, pkgPath);
        }

        Output.OperationEnd($"Packaged installer: {pkgPath}");
    }

    private static void BuildUniversalAppBundle(string pkgPath)
    {
        // Build x64 and arm64 app bundles. We will later merge these together to form the universal bundle.
        Cli.Run("dotnet", "build NAPS2.App.Mac -c InstallerEXE -r osx-x64");
        Cli.Run("dotnet", "build NAPS2.App.Mac -c InstallerEXE -r osx-arm64");

        var binRoot = Path.Combine(Paths.SolutionRoot, "NAPS2.App.Mac", "bin", "InstallerEXE", "net7-macos10.15");
        var destApp = Path.Combine(binRoot, "osx-universal", "NAPS2.app");
        var intelApp = Path.Combine(binRoot, "osx-x64", "NAPS2.app");
        var armApp = Path.Combine(binRoot, "osx-arm64", "NAPS2.app");
        var destContents = Path.Combine(destApp, "Contents");
        var intelContents = Path.Combine(intelApp, "Contents");
        var armContents = Path.Combine(armApp, "Contents");

        Output.Verbose($"Creating universal app: {destApp}");

        // Clean up previously-built bundles
        if (Directory.Exists(destApp))
        {
            Directory.Delete(destApp, true);
        }
        Directory.CreateDirectory(destApp);

        // For the top-level executable, we can merge the x64 and arm64 exes into a single binary with the "lipo" tool
        // https://developer.apple.com/documentation/apple-silicon/building-a-universal-macos-binary
        var intelExe = ChangeExecutableLibPath(intelContents, "x64", UNIVERSAL_BUNDLE_X64_LIBS);
        var armExe = ChangeExecutableLibPath(armContents, "arm64", UNIVERSAL_BUNDLE_ARM64_LIBS);
        Directory.CreateDirectory(Path.Combine(destContents, "MacOS"));
        var destExe = Path.Combine(destContents, "MacOS", "NAPS2");
        Cli.Run("lipo", $"-create -output \"{destExe}\" \"{intelExe}\" \"{armExe}\"");

        // Copy the library files (.dll, .dylib) which are normally stored in "MonoBundle" into separate library folders
        // for each arch. We already called ChangeExecutableLibPath to point the exes to the new folders.
        CopyDirectory(
            Path.Combine(intelContents, "MonoBundle"),
            Path.Combine(destContents, UNIVERSAL_BUNDLE_X64_LIBS), true);
        CopyDirectory(
            Path.Combine(armContents, "MonoBundle"),
            Path.Combine(destContents, UNIVERSAL_BUNDLE_ARM64_LIBS), true);

        // Copy resources and metadata files
        CopyDirectory(
            Path.Combine(intelContents, "Resources"),
            Path.Combine(destContents, "Resources"), true);
        CopyDirectory(
            Path.Combine(armContents, "Resources"),
            Path.Combine(destContents, "Resources"), true);
        CopyDirectory(intelContents, destContents, false);

        // Finally we run the "productbuild" tool which generates a .pkg installer from a .app bundle
        Output.Verbose($"Building installer package");
        Cli.Run("productbuild", $"--component \"{destApp}\" /Applications \"{pkgPath}\"");
    }

    private static string ChangeExecutableLibPath(string contents, string arch, string libFolderName)
    {
        // This is a complete hack. The .NET-generated root executable has hard-coded library path references to the
        // "MonoBundle" subfolder. By changing this path to something arch-specific, we can have two separate sets of
        // libraries coexist in the bundle, one for each arch.
        var exe = Path.Combine(Paths.SetupObj, $"NAPS2-{arch}");
        File.Copy(Path.Combine(contents, "MacOS", "NAPS2"), exe, true);
        // Note that it's important that the new folder name have the same number of characters as "MonoBundle" so it
        // doesn't mess up any offsets in the binary.
        ReplaceInFile(exe, "MonoBundle", libFolderName);
        // As we've modified the binary, this invalidates the existing signature so it needs to be updated.
        Cli.Run("codesign", $"-s test-selfsign {exe}");
        return exe;
    }

    private static void ReplaceInFile(string file, string search, string replace)
    {
        var s = Encoding.UTF8.GetBytes(search);
        var r = Encoding.UTF8.GetBytes(replace);
        if (search.Length != replace.Length) throw new ArgumentException();
        var data = File.ReadAllBytes(file);
        for (int i = 0; i < data.Length - s.Length; i++)
        {
            bool matches = true;
            for (int j = 0; j < s.Length; j++)
            {
                if (data[i + j] != s[j])
                {
                    matches = false;
                    break;
                }
            }
            if (!matches) continue;
            for (int j = 0; j < s.Length; j++)
            {
                data[i + j] = r[j];
            }
        }
        File.WriteAllBytes(file, data);
    }

    private static void CopyDirectory(string source, string dest, bool recursive)
    {
        CopyDirectory(new DirectoryInfo(source), new DirectoryInfo(dest), recursive);
    }

    private static void CopyDirectory(DirectoryInfo source, DirectoryInfo dest, bool recursive)
    {
        dest.Create();
        if (recursive)
        {
            foreach (var sub in source.EnumerateDirectories())
            {
                CopyDirectory(sub, dest.CreateSubdirectory(sub.Name), true);
            }
        }
        foreach (var file in source.EnumerateFiles())
        {
            file.CopyTo(Path.Combine(dest.FullName, file.Name), true);
        }
    }
}