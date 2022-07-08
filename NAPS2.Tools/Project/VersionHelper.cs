using System.Text.RegularExpressions;

namespace NAPS2.Tools.Project;

public static class VersionHelper
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
}