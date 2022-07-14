namespace NAPS2.Tools.Project.Verification;

public class InstallCommand
{
    public static int Run(InstallOptions opts)
    {
        var platform = PlatformHelper.FromOption(opts.Platform, Platform.Win64);
        var version = ProjectHelper.GetDefaultProjectVersion();

        if (opts.What == "exe")
        {
            ExeInstaller.Install(platform, version, opts.Verbose);
        }
        if (opts.What == "msi")
        {
            MsiInstaller.Install(platform, version, opts.Verbose);
        }
        return 0;
    }
}