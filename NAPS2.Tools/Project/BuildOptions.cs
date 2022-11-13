using CommandLine;

namespace NAPS2.Tools.Project;

[Verb("build", HelpText = "Builds the project, 'build {all|debug|exe|msi|zip}'")]
public class BuildOptions : OptionsBase
{
    [Value(0, MetaName = "build type", Required = true, HelpText = "all|debug|exe|msi|zip")]
    public string? BuildType { get; set; }
}