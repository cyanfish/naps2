using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Verification;

public static class VerifyCommand
{
    public static int Run(VerifyOptions opts)
    {
        var version = ProjectHelper.GetDefaultProjectVersion();
        
        using var appDriverRunner = AppDriverRunner.Start(opts.Verbose);

        var constraints = new TargetConstraints
        {
            AllowMultiplePlatforms = true
        };
        foreach (var target in TargetsHelper.Enumerate(opts.BuildType, opts.Platform, constraints))
        {
            switch (target.BuildType)
            {
                case BuildType.Exe:
                    ExeSetupVerifier.Verify(target.Platform, version, opts.Verbose);
                    break;
                case BuildType.Msi:
                    MsiSetupVerifier.Verify(target.Platform, version, opts.Verbose);
                    break;
                case BuildType.Zip:
                    ZipArchiveVerifier.Verify(target.Platform, version, opts.Verbose, opts.NoCleanup);
                    break;
            }
        }
        return 0;
    }
}