using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project;

public class BuildCommand : ICommand<BuildOptions>
{
    public int Run(BuildOptions opts)
    {
        if (opts.BuildType?.ToLowerInvariant() == "sdk")
        {
            Cli.Run("dotnet", "publish NAPS2.Sdk.Worker/NAPS2.Sdk.Worker.Build.csproj -c Release");
        }
        foreach (var target in TargetsHelper.EnumerateBuildTargets(opts.BuildType))
        {
            var config = GetConfig(target);
            if (opts.Debug)
            {
                config += " /p:AddDebugConstant=1";
            }
            Output.Info($"Building: {config}");
            try
            {
                Cli.Run("dotnet", $"build -c {config}");
                if (OperatingSystem.IsMacOS())
                {
                    // TODO: Figure out why we need to build twice to get native deps in the output
                    Cli.Run("dotnet", $"build -c {config}");
                }
            }
            catch (Exception)
            {
                Output.Info("Build failed, retrying once");
                Cli.Run("dotnet", $"build -c {config}");
            }
            Output.OperationEnd($"Built: {config}");
        }
        return 0;
    }

    private static string GetConfig(BuildType buildType) => buildType switch
    {
        BuildType.Debug => OperatingSystem.IsMacOS()
            ? "Debug-Mac"
            : OperatingSystem.IsLinux()
                ? "Debug-Linux"
                : "Debug-Windows",
        BuildType.Release => OperatingSystem.IsMacOS()
            ? "Release-Mac"
            : OperatingSystem.IsLinux()
                ? "Release-Linux"
                : "Release-Windows",
        BuildType.Msi => "Release-Msi",
        BuildType.Zip => "Release-Zip",
        BuildType.Sdk => "Sdk",
        _ => throw new ArgumentException()
    };
}