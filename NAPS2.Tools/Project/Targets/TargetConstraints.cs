namespace NAPS2.Tools.Project.Targets;

public record TargetConstraints
{
    public bool InstallersOnly { get; set; }

    public bool AllowDebug { get; set; }

    public bool AllowMultiplePlatforms { get; set; }

    public bool RequireBuildablePlatform { get; set; }
}