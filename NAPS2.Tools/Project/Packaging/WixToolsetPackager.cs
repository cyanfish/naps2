using System.Text;
using System.Text.RegularExpressions;
using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Packaging;

public static class WixToolsetPackager
{
    public static void PackageMsi(Func<PackageInfo> pkgInfoFunc, bool noSign)
    {
        Output.Verbose("Building binaries");
        Cli.Run("dotnet", "clean NAPS2.App.Worker -c Release");
        Cli.Run("dotnet", "clean NAPS2.App.WinForms -c Release");
        Cli.Run("dotnet", "clean NAPS2.App.Console -c Release");
        Cli.Run("dotnet", "publish NAPS2.App.Worker -c Release /p:DebugType=None /p:DebugSymbols=false /p:DefineConstants=MSI");
        Cli.Run("dotnet", "publish NAPS2.App.WinForms -c Release /p:DebugType=None /p:DebugSymbols=false /p:DefineConstants=MSI");
        Cli.Run("dotnet", "publish NAPS2.App.Console -c Release /p:DebugType=None /p:DebugSymbols=false /p:DefineConstants=MSI");

        var pkgInfo = pkgInfoFunc();
        if (!noSign)
        {
            Output.Verbose("Signing contents");
            WindowsSigning.SignContents(pkgInfo);
        }

        var msiPath = pkgInfo.GetPath("msi");
        Output.Info($"Packaging msi installer: {msiPath}");

        var wxsPath = GenerateWxs(pkgInfo);

        var candle = Environment.ExpandEnvironmentVariables("%PROGRAMFILES(X86)%/WiX Toolset v3.11/bin/candle.exe");
        var arch = pkgInfo.Platform == Platform.Win64 ? "x64" : "x86";
        Cli.Run(candle, $"\"{wxsPath}\" -o \"{Paths.SetupObj}/\" -arch {arch}");

        var wixobjPath = wxsPath.Replace(".wxs", ".wixobj");

        var light = Environment.ExpandEnvironmentVariables("%PROGRAMFILES(X86)%/WiX Toolset v3.11/bin/light.exe");
        Cli.Run(light, $"\"{wixobjPath}\" -spdb -ext WixUIExtension -o \"{msiPath}\"");

        if (!noSign)
        {
            Output.Verbose("Signing installer");
            WindowsSigning.SignFile(msiPath);
        }

        Output.OperationEnd($"Packaged msi installer: {msiPath}");
    }

    private static string GenerateWxs(PackageInfo packageInfo)
    {
        var template = File.ReadAllText(Path.Combine(Paths.SetupWindows, "setup.template.wxs"));

        template = template.Replace("{{ !version }}", packageInfo.VersionNumber);

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

        var langRefsLines = new StringBuilder();
        var langFilesLines = new StringBuilder();
        foreach (var language in packageInfo.Languages.OrderBy(x => x))
        {
            var id = ToId(language);
            langFilesLines.AppendLine($"<Directory Id=\"LangFolder_{id}\" Name=\"{language}\">");
            foreach (var langResource in packageInfo.Files.Where(x => x.DestDir == Path.Combine("lib", language)))
            {
                var componentId = $"LangComponent_{ToId(langResource.DestPath)}";
                langRefsLines.AppendLine($"<ComponentRef Id=\"{componentId}\" />");
                langFilesLines.AppendLine($"<Component Id=\"{componentId}\" Guid=\"{Guid.NewGuid()}\" >");
                DeclareFile(langFilesLines, langResource);
                langFilesLines.AppendLine("</Component>");
            }
            langFilesLines.AppendLine("</Directory>");
        }
        template = template.Replace("<!-- !langrefs -->", langRefsLines.ToString());
        template = template.Replace("<!-- !langfiles -->", langFilesLines.ToString());

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
        var fileId = ToId(file.DestPath);
        output.Append($" Id=\"{fileId}\"");
        output.AppendLine(" />");
    }

    private static string ToId(string raw)
    {
        return Regex.Replace(raw, @"[^a-zA-Z0-9]+", "_");
    }
}