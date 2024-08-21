using System.Text;

namespace NAPS2.Tools.Project.Packaging;

public static class InnoSetupPackager
{
    public static void PackageExe(Func<PackageInfo> pkgInfoFunc)
    {
        Output.Verbose("Building binaries");
        Cli.Run("dotnet", "clean NAPS2.App.Worker -c Release");
        Cli.Run("dotnet", "clean NAPS2.App.WinForms -c Release");
        Cli.Run("dotnet", "clean NAPS2.App.Console -c Release");
        Cli.Run("dotnet", "publish NAPS2.App.Worker -c Release /p:DebugType=None /p:DebugSymbols=false");
        Cli.Run("dotnet", "publish NAPS2.App.WinForms -c Release /p:DebugType=None /p:DebugSymbols=false");
        Cli.Run("dotnet", "publish NAPS2.App.Console -c Release /p:DebugType=None /p:DebugSymbols=false");

        var pkgInfo = pkgInfoFunc();
        var exePath = pkgInfo.GetPath("exe");
        Output.Info($"Packaging exe installer: {exePath}");

        var innoDefPath = GenerateInnoDef(pkgInfo);

        // TODO: Use https://github.com/DomGries/InnoDependencyInstaller for .net dependency
        var iscc = Environment.ExpandEnvironmentVariables("%PROGRAMFILES(X86)%/Inno Setup 6/iscc.exe");
        Cli.Run(iscc, $"\"{innoDefPath}\"");

        Output.OperationEnd($"Packaged exe installer: {exePath}");
    }

    private static string GenerateInnoDef(PackageInfo packageInfo)
    {
        var template = File.ReadAllText(Path.Combine(Paths.SetupWindows, "setup.template.iss"));

        var defLines = new StringBuilder();
        defLines.AppendLine($"#define AppVersion \"{packageInfo.VersionNumber}\"");
        defLines.AppendLine($"#define AppVersionName \"{packageInfo.VersionName}\"");
        defLines.AppendLine($"#define AppPlatform \"{packageInfo.PackageName}\"");
        template = template.Replace("; !defs", defLines.ToString());

        var arch = new StringBuilder();
        arch.AppendLine("ArchitecturesInstallIn64BitMode=x64");
        template = template.Replace("; !clean32", @"Type: filesandordirs; Name: ""{commonpf32}\NAPS2""");
        arch.AppendLine("ArchitecturesAllowed=x64");
        template = template.Replace("; !arch", arch.ToString());

        var fileLines = new StringBuilder();
        foreach (var pkgFile in packageInfo.Files)
        {
            fileLines.Append(@$"Source: ""{pkgFile.SourcePath}""; ");
            var destDir = pkgFile.DestDir == "" ? "{app}" : "{app}\\" + pkgFile.DestDir;
            fileLines.Append(@$"DestDir: ""{destDir}""; ");
            if (pkgFile.DestFileName != null)
            {
                fileLines.Append(@$"DestName: ""{pkgFile.DestFileName}""; ");
            }
            fileLines.AppendLine("Flags: ignoreversion");
        }
        template = template.Replace("; !files", fileLines.ToString());

        var innoDefPath = Path.Combine(Paths.SetupObj, "setup.iss");
        File.WriteAllText(innoDefPath, template);
        return innoDefPath;
    }
}