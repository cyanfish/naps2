using CommandLine;

namespace NAPS2.Tools.Project.Releasing;

[Verb("setver", HelpText = "Set the version everywhere, e.g. 'setver 7.0b1' or 'setver 7.1.0'")]
public class SetVersionOptions : OptionsBase
{
    [Value(0, MetaName = "version name", Required = true)]
    public string? VersionName { get; set; }
}