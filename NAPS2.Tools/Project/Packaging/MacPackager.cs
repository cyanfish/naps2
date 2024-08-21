using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Packaging;

public static class MacPackager
{
    // TODO: See if we can use some of the Xamarin codesigning features. Unfortunately there's zero doc for dotnet.
    // Some reference:
    // https://github.com/xamarin/xamarin-macios/issues/15745

    public static void Package(PackageInfo packageInfo, bool noSign, bool noNotarize)
    {
        var pkgPath = packageInfo.GetPath("pkg");
        Output.Info($"Packaging installer: {pkgPath}");

        Output.Verbose("Building bundle");
        var basePath = Path.Combine(Paths.SolutionRoot, "NAPS2.App.Mac", "bin", "Release", "net8-macos");
        string bundlePath = packageInfo.Platform switch
        {
            Platform.Mac => Path.Combine(basePath, "NAPS2.app"),
            Platform.MacIntel => Path.Combine(basePath, "osx-x64", "NAPS2.app"),
            Platform.MacArm => Path.Combine(basePath, "osx-arm64", "NAPS2.app"),
            _ => throw new InvalidOperationException()
        };
        if (Directory.Exists(bundlePath))
        {
            Directory.Delete(bundlePath, true);
        }
        Cli.Run("dotnet", $"build NAPS2.App.Mac -c Release");
        if (packageInfo.Platform != Platform.Mac)
        {
            // By default resource files are only copied into the universal bundle.
            // We also need to run this to copy into the arch-specific bundle.
            // (And we still have to build above as weirdly this command on its own ONLY copies resources.)
            var runtimeId = packageInfo.Platform == Platform.MacArm ? "osx-arm64" : "osx-x64";
            Cli.Run("dotnet", $"build NAPS2.App.Mac -c Release -r {runtimeId}");
        }

        Output.Verbose("Building package");
        var applicationIdentity = noSign ? "" : N2Config.MacApplicationIdentity;
        if (string.IsNullOrEmpty(applicationIdentity) && !noSign)
        {
            Output.Info(
                "Skipping application signing as mac-application-identity is not present in NAPS2.Tools/n2-config.json");
        }
        var installerIdentity = noSign ? "" : N2Config.MacInstallerIdentity;
        if (string.IsNullOrEmpty(installerIdentity) && !noSign)
        {
            Output.Info(
                "Skipping installer signing as mac-installer-identity is not present in NAPS2.Tools/n2-config.json");
        }
        if (!string.IsNullOrEmpty(applicationIdentity))
        {
            SignBundleContents(bundlePath, applicationIdentity);
            var mainExe = Path.Combine(bundlePath, "Contents", "MacOS", "NAPS2");
            var entitlements = Path.Combine(Paths.SolutionRoot, "NAPS2.App.Mac", "Entitlements.plist");
            Cli.Run("codesign",
                $"-s \"{applicationIdentity}\" \"{mainExe}\" -f --options runtime --entitlements \"{entitlements}\"");
        }

        var tesseractPath1 = Path.Combine(bundlePath, "Contents", "Resources", "_mac", "tesseract");
        if (Path.Exists(tesseractPath1))
        {
            Cli.Run("chmod", $"+x \"{tesseractPath1}\"");
        }
        var tesseractPath2 = Path.Combine(bundlePath, "Contents", "Resources", "_macarm", "tesseract");
        if (Path.Exists(tesseractPath2))
        {
            Cli.Run("chmod", $"+x \"{tesseractPath2}\"");
        }

        var signArgs = string.IsNullOrEmpty(installerIdentity) ? "" : $"--sign \"{installerIdentity}\"";
        Cli.Run("productbuild", $"--component \"{bundlePath}\" /Applications {signArgs} \"{pkgPath}\"");

        var notarizationArgs = noSign || noNotarize ? "" : N2Config.MacNotarizationArgs;
        if (string.IsNullOrEmpty(notarizationArgs) && !noSign)
        {
            Output.Info("Skipping notarization as mac-notarization-args is not present in NAPS2.Tools/n2-config.json");
        }
        if (!string.IsNullOrEmpty(notarizationArgs))
        {
            Output.Verbose("Notarizing package");
            Cli.Run("xcrun", $"notarytool submit {pkgPath} {notarizationArgs} --wait");
            Cli.Run("xcrun", $"stapler staple {pkgPath}");
        }

        Output.OperationEnd($"Packaged installer: {pkgPath}");
    }

    private static void SignBundleContents(string bundlePath, string signingIdentity)
    {
        var dirInfo = new DirectoryInfo(bundlePath);
        foreach (var directory in dirInfo.EnumerateDirectories())
        {
            SignBundleContents(directory.FullName, signingIdentity);
        }
        var files = string.Join("\" \"",
            dirInfo.EnumerateFiles()
                .Where(file => file.Extension is ".dylib" or ".so" or "")
                .Select(file => file.FullName));
        if (files.Length > 0)
        {
            Cli.Run("codesign", $"-s \"{signingIdentity}\" -f --options runtime \"{files}\"");
        }
    }
}