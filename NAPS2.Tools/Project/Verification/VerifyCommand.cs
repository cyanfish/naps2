using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Verification;

public class VerifyCommand : ICommand<VerifyOptions>
{
    public int Run(VerifyOptions opts)
    {
        var version = ProjectHelper.GetCurrentVersionName();
        
        using var appDriverRunner = AppDriverRunner.Start();

        var constraints = new TargetConstraints
        {
            AllowMultiplePlatforms = true,
            RequireBuildablePlatform = true
        };
        foreach (var target in TargetsHelper.Enumerate(opts.BuildType, opts.Platform, constraints))
        {
            switch (target.BuildType)
            {
                case BuildType.Exe:
                    ExeSetupVerifier.Verify(target.Platform, version);
                    break;
                case BuildType.Msi:
                    MsiSetupVerifier.Verify(target.Platform, version);
                    break;
                case BuildType.Zip:
                    ZipArchiveVerifier.Verify(target.Platform, version, opts.NoCleanup);
                    break;
            }
        }
        return 0;
    }
}