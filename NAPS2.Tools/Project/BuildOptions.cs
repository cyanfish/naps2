using CommandLine;

namespace NAPS2.Tools.Project;

[Verb("build", HelpText = "Builds the project, 'build {all|debug|exe|msi|zip}'")]
public class BuildOptions
{
    [Value(0, MetaName = "build type", Required = true, HelpText = "all|debug|exe|msi|zip")]
    public string? BuildType { get; set; }

    [Option('v', "verbose", Required = false, HelpText = "Show full output")]
    public bool Verbose { get; set; }
}