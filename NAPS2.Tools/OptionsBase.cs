using CommandLine;

namespace NAPS2.Tools.Project;

public class OptionsBase
{
    [Option('v', "verbose", Required = false, HelpText = "Show full output")]
    public bool Verbose { get; set; }
}