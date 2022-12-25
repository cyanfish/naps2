using CommandLine;

namespace NAPS2.Tools.Project;

[Verb("test", HelpText = "Runs the project tests")]
public class TestOptions : OptionsBase
{
    [Option("nogui", Required = false, HelpText = "Only run headless (no gui) tests")]
    public bool NoGui { get; set; }
}