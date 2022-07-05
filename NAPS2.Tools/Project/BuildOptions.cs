using CommandLine;

namespace NAPS2.Tools.Project;

[Verb("build", HelpText = "Builds the project, 'build all' for everything (all configs and targets)")]
public class BuildOptions
{
    [Value(0, MetaName = "what", Required = true, HelpText = "all|debug|exe|msi|zip")]
    public string? What { get; set; }
}