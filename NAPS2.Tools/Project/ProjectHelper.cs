using System.Threading;
using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project;

public static class ProjectHelper
{
    public static string GetCurrentVersion()
    {
        var versionTargetsPath = Path.Combine(Paths.Setup, "targets", "VersionTargets.targets");
        var versionTargetsFile = XDocument.Load(versionTargetsPath);
        var version = versionTargetsFile.Descendants().SingleOrDefault(x => x.Name.LocalName == "Version")?.Value;
        if (version == null)
        {
            throw new Exception($"Could not read version from project: {versionTargetsPath}");
        }
        return version;
    }

    public static string GetCurrentVersionName()
    {
        var versionTargetsPath = Path.Combine(Paths.Setup, "targets", "VersionTargets.targets");
        var versionTargetsFile = XDocument.Load(versionTargetsPath);
        var version = versionTargetsFile.Descendants().SingleOrDefault(x => x.Name.LocalName == "VersionName")?.Value;
        if (version == null)
        {
            throw new Exception($"Could not read version from project: {versionTargetsPath}");
        }
        return version;
    }

    public static string GetPackagePath(string ext, Platform platform, string? version = null)
    {
        version ??= GetCurrentVersionName();
        var path = Path.Combine(Paths.Publish, version, $"naps2-{version}-{platform.PackageName()}.{ext}");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        return path;
    }

    public static string GetInstallationFolder(Platform platform)
    {
        var pfVar = platform == Platform.Win32 ? "%PROGRAMFILES(X86)%" : "%PROGRAMFILES%";
        var pfPath = Environment.ExpandEnvironmentVariables(pfVar);
        return Path.Combine(pfPath, "NAPS2");
    }

    public static void DeleteInstallationFolder(Platform platform)
    {
        var folder = GetInstallationFolder(platform);
        if (Directory.Exists(folder))
        {
            Output.Info($"Deleting old installation: {folder}");
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