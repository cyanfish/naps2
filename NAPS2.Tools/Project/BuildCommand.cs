using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project;

public class BuildCommand : ICommand<BuildOptions>
{
    public int Run(BuildOptions opts)
    {
        var constraints = new TargetConstraints
        {
            AllowDebug = true
        };
        foreach (var target in TargetsHelper.Enumerate(opts.BuildType, null, constraints))
        {
            var config = GetConfig(target.BuildType);
            Output.Info($"Building: {config}");
            Cli.Run("dotnet", $"build -c {config}");
            Output.OperationEnd($"Built: {config}");
        }
        return 0;
    }

    private static string GetConfig(BuildType buildType) => buildType switch
    {
        BuildType.Debug => "Debug",
        BuildType.Exe => "InstallerEXE",
        BuildType.Msi => "InstallerMSI",
        BuildType.Zip => "Standalone",
        _ => throw new ArgumentException()
    };
}