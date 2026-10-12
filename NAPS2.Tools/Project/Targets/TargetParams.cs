namespace NAPS2.Tools.Project.Targets;

public record TargetParams
{
    /// <summary>
    /// The package type(s) to include, e.g. "exe" or "exe+msi". Null or "all" for all types.
    /// </summary>
    public string? PackageType { get; init; }

    /// <summary>
    /// The platform(s) to include, e.g. "win64" or "win64+winarm". Null or "all" for all platforms.
    /// </summary>
    public string? Platform { get; init; }

    /// <summary>
    /// Only include targets that can be built and packaged on the current machine.
    /// </summary>
    public bool RequireBuildable { get; init; }

    /// <summary>
    /// Only include targets whose packages can be installed and run on the current machine.
    /// </summary>
    public bool RequireRunnable { get; init; }

    /// <summary>
    /// With RequireBuildable, also include targets that can be cross-compiled for another architecture.
    /// </summary>
    public bool XCompile { get; init; }
}
