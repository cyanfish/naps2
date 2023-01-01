using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Packaging;

public static class FlatpakPackager
{
    public static void Package(PackageInfo packageInfo, bool noPre)
    {
        // TODO: This only uses committed files, which is different from other packagers - maybe we can fix that somehow

        var bundlePath = packageInfo.GetPath("flatpak");
        Output.Info($"Packaging flatpak: {bundlePath}");

        VerifyCanBuildArch(packageInfo.Platform);

        // Update metainfo file with the current version/date
        var metaInfo = File.ReadAllText(Path.Combine(Paths.SetupLinux, "com.naps2.Naps2.metainfo.xml"));
        var version = ProjectHelper.GetCurrentVersionName();
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        metaInfo = Regex.Replace(metaInfo,
            @"<release [^>]+/>",
            $"<release version=\"{version}\" date=\"{date}\" />");

        // Update manifest file with the correct paths
        var manifest = File.ReadAllText(Path.Combine(Paths.SetupLinux, "com.naps2.Naps2.yml"));
        // TODO: Update this after we use a real repo path
        manifest = manifest.Replace("../../../../../..", Paths.SolutionRoot);

        // Copy metainfo, manifest, icon, and desktop files to a temp folder
        var packageDir = Path.Combine(Paths.SetupObj, "flatpak");
        Directory.CreateDirectory(packageDir);
        File.WriteAllText(Path.Combine(packageDir, "com.naps2.Naps2.metainfo.xml"), metaInfo);
        File.WriteAllText(Path.Combine(packageDir, "com.naps2.Naps2.yml"), manifest);
        File.Copy(
            Path.Combine(Paths.SetupLinux, "com.naps2.Naps2.desktop"),
            Path.Combine(packageDir, "com.naps2.Naps2.desktop"), true);
        File.Copy(
            Path.Combine(Paths.SolutionRoot, "NAPS2.Lib", "Icons", "scanner-128.png"),
            Path.Combine(packageDir, "com.naps2.Naps2.png"), true);

        if (!noPre)
        {
            // Run pre-processing to generate a nuget-sources.json file with all the packages we need to build
            // This is needed as the build sandbox doesn't have internet access
            // TODO: Maybe have an option to just restore for the current arch? When we do an upload we definitely want all
            // of them, but for just building a single file we only need the current
            Output.Verbose("Generating nuget sources");
            var scriptPath = Path.Combine(Paths.SetupLinux, "flatpak-dotnet-generator.py");
            var nugetSourcesPath = Path.Combine(packageDir, "nuget-sources.json");
            var projectPath = Path.Combine(Paths.SolutionRoot, "NAPS2.App.Gtk", "NAPS2.App.Gtk.csproj");
            Cli.Run("python3", $"{scriptPath} {nugetSourcesPath} {projectPath}");
        }

        // Do the actual flatpak build
        Output.Verbose("Running flatpak build");
        var buildDir = Path.Combine(packageDir, "build-dir");
        var manifestPath = Path.Combine(packageDir, "com.naps2.Naps2.yml");
        var arch = packageInfo.Platform switch
        {
            Platform.LinuxArm => "aarch64",
            _ => "x86_64"
        };
        var stateDir = Path.Combine(packageDir, "builder-state");
        Cli.Run("flatpak-builder", $"--arch {arch} --force-clean --state-dir {stateDir} {buildDir} {manifestPath}");

        // Generate a temp repo with the package info
        Output.Verbose("Creating flatpak repo");
        var repoDir = Path.Combine(packageDir, "repo");
        Cli.Run("flatpak", $"build-export --arch {arch} {repoDir} {buildDir}");

        // Generate a single-file bundle from the temp repo
        Output.Verbose("Building flatpak bundle");
        Cli.Run("flatpak", $"build-bundle --arch {arch} {repoDir} {bundlePath} com.naps2.Naps2");

        Output.OperationEnd($"Packaged flatpak: {bundlePath}");
    }

    private static void VerifyCanBuildArch(Platform platform)
    {
        if (platform == Platform.Linux && RuntimeInformation.OSArchitecture != Architecture.X64)
        {
            Cli.Run("qemu-x86_64-static", "--version");
        }
        if (platform == Platform.LinuxArm && RuntimeInformation.OSArchitecture != Architecture.Arm64)
        {
            Cli.Run("qemu-aarch64-static", "--version");
        }
    }
}