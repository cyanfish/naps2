using System.Text;

namespace NAPS2.Tools.Project.Packaging;

public static class InnoSetupPackager
{
    public static void PackageExe(PackageInfo packageInfo)
    {
        var innoDefPath = GenerateInnoDef(packageInfo);

        // TODO: Use https://github.com/DomGries/InnoDependencyInstaller for .net dependency
        var iscc = Environment.ExpandEnvironmentVariables("%PROGRAMFILES(X86)%/Inno Setup 6/iscc.exe");
        Cli.Run(iscc, $"\"{innoDefPath}\"");
    }

    private static string GenerateInnoDef(PackageInfo packageInfo)
    {
        var template = File.ReadAllText(Path.Combine(Paths.Setup, "setup.template.iss"));

        var defLines = new StringBuilder();
        defLines.AppendLine($"#define AppVersion \"{packageInfo.Version}\"");
        defLines.AppendLine($"#define AppPlatform \"{packageInfo.Platform.PackageName()}\"");
        template = template.Replace("; !defs", defLines.ToString());

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