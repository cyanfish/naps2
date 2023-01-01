using NAPS2.Tools.Project.Packaging;
using NAPS2.Tools.Project.Verification;

namespace NAPS2.Tools.Project.Workflows;

public class PublishCommand : ICommand<PublishOptions>
{
    public int Run(PublishOptions opts)
    {
        new CleanCommand().Run(new CleanOptions());
        new BuildCommand().Run(new BuildOptions
        {
            BuildType = opts.BuildType
        });
        new TestCommand().Run(new TestOptions());
        new PackageCommand().Run(new PackageOptions
        {
            BuildType = opts.BuildType,
            Platform = opts.Platform
        });
        if (!opts.NoVerify)
        {
            new VerifyCommand().Run(new VerifyOptions
            {
                BuildType = opts.BuildType,
                Platform = opts.Platform
            });
        }
        return 0;
    }
}