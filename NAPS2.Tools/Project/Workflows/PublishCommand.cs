using NAPS2.Tools.Project.Packaging;
using NAPS2.Tools.Project.Verification;

namespace NAPS2.Tools.Project.Workflows;

public static class PublishCommand
{
    public static int Run(PublishOptions opts)
    {
        CleanCommand.Run(new CleanOptions
        {
            Verbose = opts.Verbose
        });
        BuildCommand.Run(new BuildOptions
        {
            BuildType = opts.BuildType,
            Verbose = opts.Verbose
        });
        TestCommand.Run(new TestOptions
        {
            Verbose = opts.Verbose
        });
        PackageCommand.Run(new PackageOptions
        {
            BuildType = opts.BuildType,
            Platform = opts.Platform,
            Verbose = opts.Verbose
        });
        VerifyCommand.Run(new VerifyOptions
        { 
            BuildType = opts.BuildType,
            Platform = opts.Platform,
            Verbose = opts.Verbose
        });
        return 0;
    }
}