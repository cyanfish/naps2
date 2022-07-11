namespace NAPS2.Tools.Project.Verification;

public class VerifyCommand
{
    public static int Run(VerifyOptions opts)
    {
        var platform = PlatformHelper.FromOption(opts.Platform, Platform.Win64);
        var version = ProjectHelper.GetDefaultProjectVersion();

        using var appDriverRunner = AppDriverRunner.Start(opts.Verbose);
        if (opts.What == "exe" || opts.What == "all")
        {
            ExeSetupVerifier.Verify(platform, version, opts.Verbose);
        }
        if (opts.What == "msi" || opts.What == "all")
        {
            MsiSetupVerifier.Verify(platform, version, opts.Verbose);
        }
        if (opts.What == "zip" || opts.What == "all")
        {
            ZipArchiveVerifier.Verify(platform, version, opts.NoCleanup, opts.Verbose);
        }
        return 0;
    }
}