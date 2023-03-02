using NAPS2.Tools.Project.Packaging;
using NAPS2.Tools.Project.Targets;
using NAPS2.Tools.Project.Verification;

namespace NAPS2.Tools.Project.Workflows;

public class PublishCommand : ICommand<PublishOptions>
{
    public int Run(PublishOptions opts)
    {
        new CleanCommand().Run(new CleanOptions());
        foreach (var buildType in TargetsHelper.GetBuildTypesFromPackageType(opts.PackageType))
        {
            new BuildCommand().Run(new BuildOptions
            {
                BuildType = buildType
            });
        }
        new TestCommand().Run(new TestOptions());
        new PackageCommand().Run(new PackageOptions
        {
            PackageType = opts.PackageType,
            Platform = opts.Platform
        });
        if (!opts.NoVerify)
        {
            new VerifyCommand().Run(new VerifyOptions
            {
                PackageType = opts.PackageType,
                Platform = opts.Platform
            });
        }
        return 0;
    }
}