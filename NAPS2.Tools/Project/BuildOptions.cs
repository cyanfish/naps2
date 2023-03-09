using CommandLine;

namespace NAPS2.Tools.Project;

[Verb("build", HelpText = "Builds the project, 'build {all|debug|release|msi|zip}'")]
public class BuildOptions : OptionsBase
{
    [Value(0, MetaName = "build type", Required = true, HelpText = "all|debug|release|msi|zip")]
    public string? BuildType { get; set; }

    [Option("debug", Required = false, HelpText = "Set DEBUG compile-time constant")]
    public bool Debug { get; set; }
}