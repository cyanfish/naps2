using System.Text.RegularExpressions;

namespace NAPS2.Tools.Project;

public static class ProjectHelper
{
    public static string GetProjectVersion(string projectName)
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

    public static string GetDefaultProjectVersion()
    {
        return GetProjectVersion("NAPS2.App.WinForms");
    }

    public static string GetPackagePath(string ext, Platform platform, string? version = null)
    {
        version ??= GetProjectVersion("NAPS2.App.WinForms");
        return Path.Combine(Paths.Publish, version, $"naps2-{version}-{platform.PackageName()}.{ext}");
    }

    public static string GetInstallationFolder(Platform platform)
    {
        var pfVar = platform == Platform.Win64 ? "%PROGRAMFILES%" : "%PROGRAMFILES(X86)%";
        var pfPath = Environment.ExpandEnvironmentVariables(pfVar);
        return Path.Combine(pfPath, "NAPS2");
    }
}