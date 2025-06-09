using CommandLine;

namespace NAPS2.Tools.Project;

[Verb("test", HelpText = "Runs the project tests")]
public class TestOptions : OptionsBase
{
    [Option("nogui", Required = false, HelpText = "Only run headless (no gui) tests")]
    public bool NoGui { get; set; }

    [Option("nonetwork", Required = false, HelpText = "Skip ESCL tests that require network access")]
    public bool NoNetwork { get; set; }

    [Option("images", Required = false, HelpText = "Run SDK tests with images for a particular platform (gdi/wpf/is/mac/gtk)")]
    public string? Images { get; set; }

    [Option("scope", Required = false, HelpText = "Subset of tests to run (app/lib/sdk)")]
    public string? Scope { get; set; }
}