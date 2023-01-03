using CommandLine;

namespace NAPS2.Tools.Project.Releasing;

[Verb("website", HelpText = "Update the website files")]
public class WebsiteUpdateOptions : OptionsBase
{
    [Option("version", Required = false, HelpText = "Version to sign")]
    public string? Version { get; set; }

    [Option("nosign", Required = false, HelpText = "Skip file signatures")]
    public bool NoSign { get; set; }

    [Option("norelease", Required = false, HelpText = "Skip release updates")]
    public bool NoRelease { get; set; }
}