namespace NAPS2.Tools.Project.Targets;

public record PackageTarget(PackageType Type, Platform Platform)
{
    public string PackagePath => ProjectHelper.GetPackagePath(Type.ToString().ToLowerInvariant(), Platform);
}