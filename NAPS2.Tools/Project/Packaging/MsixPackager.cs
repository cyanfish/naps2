using System.Text;

namespace NAPS2.Tools.Project.Packaging;

public static class MsixPackager
{
    public static void PackageMsix(Func<PackageInfo> pkgInfoFunc, bool noSign)
    {
        Output.Verbose("Building binaries");
        Cli.Run("dotnet", "clean NAPS2.App.Worker -c Release");
        Cli.Run("dotnet", "clean NAPS2.App.WinForms -r win-x64 -c Release");
        Cli.Run("dotnet", "clean NAPS2.App.Console -r win-x64 -c Release");
        Cli.Run("dotnet",
            "publish NAPS2.App.Worker -c Release /p:DebugType=None /p:DebugSymbols=false /p:DefineConstants=MSI");
        Cli.Run("dotnet",
            "publish NAPS2.App.WinForms -r win-x64 -c Release /p:DebugType=None /p:DebugSymbols=false /p:DefineConstants=MSI");
        Cli.Run("dotnet",
            "publish NAPS2.App.Console -r win-x64 -c Release /p:DebugType=None /p:DebugSymbols=false /p:DefineConstants=MSI");

        var pkgInfo = pkgInfoFunc();

        var msixPath = pkgInfo.GetPath("msix");
        var msixStorePath = msixPath.Replace(".msix", "-store.msix");
        Output.Info($"Packaging msix installer: {msixPath}");

        if (File.Exists(msixStorePath))
        {
            File.Delete(msixStorePath);
        }
        if (File.Exists(msixPath))
        {
            File.Delete(msixPath);
        }

        var manifestPath = Path.Combine(Paths.SetupObj, "appxmanifest.xml");
        var resourcesPriPath = Path.Combine(Paths.SetupObj, "resources.pri");
        var msixConfig = Path.Combine(Paths.SetupWindows, "msix");
        File.Copy(Path.Combine(msixConfig, "appxmanifest.xml"), manifestPath, true);
        var publishDir = Path.Combine(Paths.SolutionRoot, "NAPS2.App.WinForms", "bin", "Release", "net9", "win-x64",
            "publish");
        var mappingFilePath = Path.Combine(Paths.SetupObj, "msixmapping.txt");

        var mappingFile = new StreamWriter(new FileStream(mappingFilePath, FileMode.Create));
        mappingFile.WriteLine("[Files]");
        foreach (var file in pkgInfo.Files)
        {
            var fullSourcePath = Path.Combine(publishDir, file.SourcePath);
            mappingFile.WriteLine($"\"{fullSourcePath}\" \"{file.DestPath}\"");
        }
        foreach (var assetFile in new DirectoryInfo(Path.Combine(msixConfig, "Assets")).EnumerateFiles())
        {
            mappingFile.WriteLine($"\"{assetFile.FullName}\" \"Assets\\{assetFile.Name}\"");
        }
        mappingFile.WriteLine($"\"{manifestPath}\" \"AppxManifest.xml\"");
        mappingFile.WriteLine($"\"{resourcesPriPath}\" \"resources.pri\"");
        mappingFile.Close();

        var makePri = @"C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\makepri.exe";
        var makeAppx = @"C:\Program Files (x86)\Windows Kits\10\App Certification Kit\makeappx.exe";

        if (File.Exists(resourcesPriPath)) File.Delete(resourcesPriPath);
        Cli.Run(makePri,
            $"new /pr \"{msixConfig}\" /cf \"{msixConfig}\\priconfig.xml\" /o /of \"{resourcesPriPath}\" /mn \"{manifestPath}\"");

        Cli.Run(makeAppx, $"pack /f \"{mappingFilePath}\" /p \"{msixStorePath}\"");

        File.WriteAllText(manifestPath, File.ReadAllText(manifestPath)
            .Replace(
                "Version=\"1.0.0.0\"",
                $"Version=\"{pkgInfo.VersionNumber}.0\"")
            .Replace(
                "CN=1D624E39-8523-4AAC-B3B6-1452E653A003",
                N2Config.WindowsIdentity)
            .Replace(
                "<Resource Language=\"en-us\" />",
                GetSupportedLanguages(pkgInfo)));

        Cli.Run(makeAppx, $"pack /f \"{mappingFilePath}\" /p \"{msixPath}\"");

        if (!noSign)
        {
            Output.Verbose("Signing installer");
            WindowsSigning.SignFile(msixPath);
        }

        Output.OperationEnd($"Packaged msix installer: {msixPath}");
    }

    private static string GetSupportedLanguages(PackageInfo pkgInfo)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<Resource Language=\"en-us\" />");
        foreach (var language in pkgInfo.Files.Where(x => x.FileName.StartsWith("NAPS2.Lib.resources.dll"))
                     .Select(x => Path.GetFileName(x.DestDir)))
        {
            // MSIX expects language codes a bit different from NAPS2
            // TODO: Would it be better to use these codes internally? Would it match up more with Windows?
            var correctedLanguage = language switch
            {
                "sr" => "sr-Cyrl",
                "sr-CS" => "sr-Latn",
                _ => language
            };
            sb.AppendLine($"<Resource Language=\"{correctedLanguage}\" />");
        }
        return sb.ToString();
    }
}