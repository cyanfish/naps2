using System.Text.RegularExpressions;
using System.Threading;
using NAPS2.Tools.Project.Targets;

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

    public static void DeleteInstallationFolder(Platform platform)
    {
        var folder = GetInstallationFolder(platform);
        if (Directory.Exists(folder))
        {
            Console.WriteLine($"Deleting old installation: {folder}");
            try
            {
                Directory.Delete(folder, true);
            }
            catch (UnauthorizedAccessException e)
            {
                // TODO: Once we add more complex workflows, we should verify elevation before running the workflow
                throw new Exception("This command requires administrator permissions to run.", e);
            }
        }
    }

    public static void CloseMostRecentNaps2()
    {
        for (int i = 0; i < 20; i++)
        {
            var proc = Process.GetProcessesByName("NAPS2")
                .Where(x => x.StartTime > DateTime.Now - TimeSpan.FromSeconds(2))
                .OrderBy(x => x.StartTime)
                .FirstOrDefault();
            if (proc != null)
            {
                proc.Kill();
                break;
            }
            Thread.Sleep(100);
        }
    }
}