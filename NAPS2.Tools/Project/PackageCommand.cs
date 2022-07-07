using System.Text;
using System.Text.RegularExpressions;

namespace NAPS2.Tools.Project;

public static class PackageCommand
{
    public static int Run(PackageOptions opts)
    {
        // TODO: Do this type of thing in the publish workflow.
        // BuildCommand.Run(new BuildOptions
        // {
        //     What = opts.What switch
        //     {
        //         "all" => "all",
        //         "exe" => "exe",
        //         "msi" => "msi",
        //         "zip" => "zip",
        //         "7z" => "zip",
        //         _ => ""
        //     }
        // });
        if (opts.What == "exe" || opts.What == "all")
        {
            // TODO: Allow customizing net version, platform, etc
            // TODO: The fact that we only have one project config for the app but multiple for the SDK is problematic; things will overwrite each other unless we either pull them explicitly from the right project or have a separate config or normalize things somehow to avoid needing multiple configs
            var buildPath = Path.Combine(Paths.SolutionRoot, "NAPS2.App.WinForms", "bin", "Release", "net462");
            if (!Directory.Exists(buildPath))
            {
                throw new Exception($"Could not find build path. Maybe run 'n2 build' first? {buildPath}");
            }
            var pkgInfo = GetPackageInfo(buildPath, Platform.Win64);
            PackageExe(pkgInfo);
        }
        if (opts.What == "msi" || opts.What == "all")
        {
            // PackageMsi();
        }
        if (opts.What == "zip" || opts.What == "zip")
        {
            // PackageZip();
        }
        if (opts.What == "7z" || opts.What == "7z")
        {
            // Package7z();
        }
        return 0;
    }

    private static PackageInfo GetPackageInfo(string buildPath, Platform platform)
    {
        var pkgInfo = new PackageInfo();
        var dir = new DirectoryInfo(buildPath);
        if (!dir.Exists)
        {
            throw new Exception($"Could not find path: {dir.FullName}");
        }
        foreach (var exeFile in dir.EnumerateFiles("*.exe"))
        {
            var dest = exeFile.Name.ToLower() switch
            {
                "naps2.worker.exe" => "lib",
                _ => ""
            };
            pkgInfo.Files.Add(new PackageFile(exeFile.DirectoryName ?? "", dest, exeFile.Name));
        }
        foreach (var configFile in dir.EnumerateFiles("*.exe.config"))
        {
            var dest = configFile.Name.ToLower() switch
            {
                "naps2.worker.exe.config" => "lib",
                _ => ""
            };
            pkgInfo.Files.Add(new PackageFile(configFile.DirectoryName ?? "", dest, configFile.Name));
        }
        foreach (var dllFile in dir.EnumerateFiles("*.dll"))
        {
            // TODO: Blacklist unneeded dlls
            pkgInfo.Files.Add(new PackageFile(dllFile.DirectoryName ?? "", "lib", dllFile.Name));
        }
        if (platform == Platform.Win32)
        {
            AddPlatformFiles(pkgInfo, buildPath, "_win32");
        }
        else if (platform == Platform.Win64)
        {
            // Special case as we have a 64 bit main app and a 32 bit worker
            AddPlatformFile(pkgInfo, buildPath, "_win32", "NAPS2.Wia.Native.dll");
            AddPlatformFile(pkgInfo, buildPath, "_win64", "NAPS2.Wia.Native.dll");
            AddPlatformFile(pkgInfo, buildPath, "_win32", "twaindsm.dll");
            AddPlatformFile(pkgInfo, buildPath, "_win64", "twaindsm.dll");
            AddPlatformFile(pkgInfo, buildPath, "_win32", "pdfium.dll");
            AddPlatformFile(pkgInfo, buildPath, "_win64", "tesseract.exe");
        }
        else
        {
            throw new Exception("Unsupported platform");
        }
        foreach (var langFolder in dir.EnumerateDirectories().Where(x => Regex.IsMatch(x.Name, "[a-z]{2}(-[A-Za-z]+)?")))
        {
            foreach (var resourceDll in langFolder.EnumerateFiles("*.resources.dll"))
            {
                pkgInfo.Files.Add(new PackageFile(langFolder.FullName, langFolder.Name, resourceDll.Name));
            }
        }
        pkgInfo.Files.Add(new PackageFile(Paths.SolutionRoot, "", "LICENSE", "license.txt"));
        pkgInfo.Files.Add(new PackageFile(Paths.SolutionRoot, "", "CONTRIBUTORS", "contributors.txt"));
        return pkgInfo;
    }

    private static void AddPlatformFiles(PackageInfo pkgInfo, string buildPath, string platformPath)
    {
        var folder = new DirectoryInfo(Path.Combine(buildPath, platformPath));
        foreach (var file in folder.EnumerateFiles())
        {
            pkgInfo.Files.Add(new PackageFile(file.DirectoryName ?? "", platformPath, file.Name));
        }
    }

    private static void AddPlatformFile(PackageInfo pkgInfo, string buildPath, string platformPath, string fileName)
    {
        pkgInfo.Files.Add(new PackageFile(Path.Combine(buildPath, platformPath), platformPath, fileName));
    }

    private static void PackageExe(PackageInfo packageInfo)
    {
        var template = File.ReadAllText(Path.Combine(Paths.SolutionRoot, "NAPS2.Setup", "setup.template.iss"));
        var version = GetProjectVersion("NAPS2.App.WinForms");
        template = template.Replace("; !version", $"#define AppVersion \"{version}\"");

        var sb = new StringBuilder();
        foreach (var pkgFile in packageInfo.Files)
        {
            sb.Append(@$"Source: ""{pkgFile.SourcePath}""; ");
            var destDir = pkgFile.DestDir == "" ? "{app}" : "{app}\\" + pkgFile.DestDir;
            sb.Append(@$"DestDir: ""{destDir}""; ");
            if (pkgFile.DestFileName != null)
            {
                sb.Append(@$"DestName: ""{pkgFile.DestFileName}""; ");
            }
            sb.AppendLine("Flags: ignoreversion");
        }
        template = template.Replace("; !files", sb.ToString());

        var innoDefPath = Path.Combine(Paths.SolutionRoot, "NAPS2.Setup", "obj", "setup.iss");
        File.WriteAllText(innoDefPath, template);

        var iscc = Environment.ExpandEnvironmentVariables("%PROGRAMFILES(X86)%/Inno Setup 6/iscc.exe");
        Cli.Run(iscc, $"\"{innoDefPath}\"");
    }

    private static string GetProjectVersion(string projectName)
    {
        var projectPath = Path.Combine(Paths.SolutionRoot, projectName, $"{projectName}.csproj");
        var projectFile = XDocument.Load(projectPath);
        var version = projectFile.Descendants().SingleOrDefault(x => x.Name == "Version")?.Value;
        if (version == null)
        {
            throw new Exception($"Could not read version from project: {projectPath}");
        }
        if (!Regex.IsMatch(version, @"[0-9]+(\.[0-9]+){2}"))
        {
            throw new Exception($"Invalid project version: {version}");
        }
        return version;
    }
}