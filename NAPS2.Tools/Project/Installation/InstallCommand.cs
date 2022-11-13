using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Installation;

public class InstallCommand : ICommand<InstallOptions>
{
    public int Run(InstallOptions opts)
    {
        var version = ProjectHelper.GetDefaultProjectVersion();

        var constraints = new TargetConstraints
        {
            InstallersOnly = true
        };
        foreach (var target in TargetsHelper.Enumerate(opts.BuildType, opts.Platform, constraints))
        {
            switch (target.BuildType)
            {
                case BuildType.Exe:
                    if (target.Platform.IsLinux())
                    {
                        FlatpakInstaller.Install(target.Platform, version, opts.Run);
                    }
                    else if (target.Platform.IsMac())
                    {
                        // TODO: Mac install?
                    }
                    else if (target.Platform.IsWindows())
                    {
                        ExeInstaller.Install(target.Platform, version, opts.Run);
                    }
                    break;
                case BuildType.Msi:
                    MsiInstaller.Install(target.Platform, version, opts.Run);
                    break;
            }
        }
        return 0;
    }
}