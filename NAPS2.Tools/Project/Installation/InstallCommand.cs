using NAPS2.Tools.Project.Targets;

namespace NAPS2.Tools.Project.Installation;

public class InstallCommand : ICommand<InstallOptions>
{
    public int Run(InstallOptions opts)
    {
        var version = ProjectHelper.GetCurrentVersionName();

        foreach (var target in TargetsHelper.EnumeratePackageTargets(opts.PackageType, opts.Platform, true))
        {
            switch (target.Type)
            {
                case PackageType.Exe:
                    ExeInstaller.Install(target.Platform, version, opts.Run);
                    break;
                case PackageType.Msi:
                    MsiInstaller.Install(target.Platform, version, opts.Run);
                    break;
                case PackageType.Flatpak:
                    FlatpakInstaller.Install(target.Platform, version, opts.Run);
                    break;
            }
        }
        return 0;
    }
}