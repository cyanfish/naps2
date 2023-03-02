using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Verification;

public class VerifyCommand : ICommand<VerifyOptions>
{
    public int Run(VerifyOptions opts)
    {
        if (!OperatingSystem.IsWindows())
        {
            Output.Info("Verification tests are currently only supported on Windows.");
            return 0;
        }

        var version = ProjectHelper.GetCurrentVersionName();
        
        using var appDriverRunner = AppDriverRunner.Start();

        foreach (var target in TargetsHelper.EnumeratePackageTargets(opts.PackageType, opts.Platform, true))
        {
            switch (target.Type)
            {
                case PackageType.Exe:
                    ExeSetupVerifier.Verify(target.Platform, version);
                    break;
                case PackageType.Msi:
                    MsiSetupVerifier.Verify(target.Platform, version);
                    break;
                case PackageType.Zip:
                    ZipArchiveVerifier.Verify(target.Platform, version, opts.NoCleanup);
                    break;
                default:
                    Output.Info($"Unsupported package type for verification: {target.Type}");
                    break;
            }
        }
        return 0;
    }
}