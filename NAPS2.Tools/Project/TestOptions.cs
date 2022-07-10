using CommandLine;

namespace NAPS2.Tools.Project;

[Verb("test", HelpText = "Runs the project tests")]
public class TestOptions
{
    [Option('v', "verbose", Required = false, HelpText = "Show full output")]
    public bool Verbose { get; set; }
}