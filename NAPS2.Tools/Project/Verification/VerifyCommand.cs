namespace NAPS2.Tools.Project.Verification;

public class VerifyCommand
{
    public static int Run(VerifyOptions opts)
    {
        var platform = PlatformHelper.FromOption(opts.Platform, Platform.Win64);

        var version = VersionHelper.GetProjectVersion("NAPS2.App.WinForms");
        var basePath = Path.Combine(Paths.Publish, version, $"naps2-{version}-{platform.PackageName()}");

        using var appDriverRunner = AppDriverRunner.Start();
        if (opts.What == "exe" || opts.What == "all")
        {
            // ExeSetupVerifier.Verify()
        }
        if (opts.What == "msi" || opts.What == "all")
        {
            // MsiSetupVerifier.Verify()
        }
        if (opts.What == "zip" || opts.What == "all")
        {
            ZipArchiveVerifier.Verify(basePath + ".zip", opts.NoCleanup);
        }
        return 0;
    }
}