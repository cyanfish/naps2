using System.Text;
using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Packaging;

public static class InnoSetupPackager
{
    public static void PackageExe(PackageInfo packageInfo)
    {
        var exePath = packageInfo.GetPath("exe");
        Output.Info($"Packaging exe installer: {exePath}");

        var innoDefPath = GenerateInnoDef(packageInfo);

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
        if (packageInfo.Platform is Platform.Win64 or Platform.Win)
        {
            arch.AppendLine("ArchitecturesInstallIn64BitMode=x64");
            template = template.Replace("; !clean32", @"Type: filesandordirs; Name: ""{commonpf32}\NAPS2""");
        }
        if (packageInfo.Platform == Platform.Win64)
        {
            arch.AppendLine("ArchitecturesAllowed=x64");
        }
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