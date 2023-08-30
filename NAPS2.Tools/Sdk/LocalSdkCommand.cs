using NAPS2.Tools.Project;

namespace NAPS2.Tools.Sdk;

public class LocalSdkCommand : ICommand<LocalSdkOptions>
{
    public int Run(LocalSdkOptions opts)
    {
        Output.Info("Generating packages");
        if (!opts.NoBuild)
        {
            new BuildCommand().Run(new BuildOptions { BuildType = "sdk" });
        }

        var sdkVersion = ProjectHelper.GetSdkVersion();
        foreach (var project in ProjectHelper.GetSdkProjects())
        {
            var projectFolder = project.StartsWith("NAPS2.Sdk.Worker") ? "NAPS2.Sdk.Worker" : project;
            var nugetPath = Path.Combine(Paths.SolutionRoot, projectFolder, "bin", "Release",
                $"{project}.{sdkVersion}.nupkg");
            Cli.Run("nuget", $"delete {project} {sdkVersion} -source \"{opts.LocalSource}\" -NonInteractive", ignoreErrorIfOutputContains: "Not Found");
            Cli.Run("nuget", $"add \"{nugetPath}\" -source \"{opts.LocalSource}\"");
        }

        Output.OperationEnd("Packages added.");
        return 0;
    }
}