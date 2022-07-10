using System.Text;
using System.Text.RegularExpressions;

namespace NAPS2.Tools.Project.Packaging;

public static class WixToolsetPackager
{
    public static void PackageMsi(PackageInfo pkgInfo, bool verbose)
    {
        var msiPath = pkgInfo.GetPath("msi");
        Console.WriteLine($"Packaging msi installer: {msiPath}");
        var wxsPath = GenerateWxs(pkgInfo);

        var candle = Environment.ExpandEnvironmentVariables("%PROGRAMFILES(X86)%/WiX Toolset v3.11/bin/candle.exe");
        Cli.Run(candle, $"\"{wxsPath}\" -o \"{Paths.SetupObj}/\"", verbose);
        
        var wixobjPath = wxsPath.Replace(".wxs", ".wixobj");

        var light = Environment.ExpandEnvironmentVariables("%PROGRAMFILES(X86)%/WiX Toolset v3.11/bin/light.exe");
        Cli.Run(light, $"\"{wixobjPath}\" -spdb -ext WixUIExtension -o \"{msiPath}\"", verbose);
        Console.WriteLine(verbose ? $"Packaged msi installer: {msiPath}" : "Done.");
    }

    private static string GenerateWxs(PackageInfo packageInfo)
    {
        // TODO: Delete Setup.Msi project
        var template = File.ReadAllText(Path.Combine(Paths.SolutionRoot, "NAPS2.Setup", "setup.template.wxs"));

        template = template.Replace("{{ !version }}", packageInfo.Version);
        
        var rootLines = new StringBuilder();
        foreach (var rootFile in packageInfo.Files.Where(x => x.DestDir == ""))
        {
            DeclareFile(rootLines, rootFile);
        }
        template = template.Replace("<!-- !root -->", rootLines.ToString());
        
        var libLines = new StringBuilder();
        foreach (var libFile in packageInfo.Files.Where(x => x.DestDir == "lib"))
        {
            DeclareFile(libLines, libFile);
        }
        template = template.Replace("<!-- !lib -->", libLines.ToString());
        
        var win32Lines = new StringBuilder();
        foreach (var win32File in packageInfo.Files.Where(x => x.DestDir == Path.Combine("lib", "_win32")))
        {
            DeclareFile(win32Lines, win32File);
        }
        template = template.Replace("<!-- !win32 -->", win32Lines.ToString());
        
        var win64Lines = new StringBuilder();
        foreach (var win64File in packageInfo.Files.Where(x => x.DestDir == Path.Combine("lib", "_win64")))
        {
            DeclareFile(win64Lines, win64File);
        }
        template = template.Replace("<!-- !win64 -->", win64Lines.ToString());

        if (packageInfo.Platform == Platform.Win32)
        {
            template = Regex.Replace(template, "<!-- !64 -->.*?<!-- !~64 -->", "", RegexOptions.Singleline);
        }
        
        // TODO: Lang components

        var wxsPath = Path.Combine(Paths.SetupObj, "setup.wxs");
        File.WriteAllText(wxsPath, template);
        return wxsPath;
    }

    private static void DeclareFile(StringBuilder output, PackageFile file)
    {
        output.Append($"<File Source=\"{file.SourcePath}\"");
        if (file.DestFileName != null)
        {
            output.Append($" Name=\"{file.DestFileName}\"");
        }
        var fileId = Regex.Replace(file.DestPath, @"[^a-zA-Z0-9]+", "_");
        output.Append($" Id=\"{fileId}\"");
        output.AppendLine(" />");
    }
}