using CommandLine;

namespace NAPS2.Tools.Project;

[Verb("scenario", HelpText = "Runs a GUI test scenario in a Debug build, 'scenario --name=desktop-images' or 'scenario --path=script.json'")]
public class ScenarioOptions : OptionsBase
{
    [Option("name", Required = false, HelpText = "The name of a scenario (e.g. NAPS2.Lib/TestScenarios/CommonTestScenarios.cs; see --list)")]
    public string? Name { get; set; }

    [Option("path", Required = false, HelpText = "The path to a JSON scenario script (see NAPS2.Lib/TestScenarios/ScriptTestScenario.cs)")]
    public string? ScriptPath { get; set; }

    [Option("list", Required = false, HelpText = "List the scenarios")]
    public bool List { get; set; }

    [Option('o', "out", Required = false, HelpText = "The app data folder, with outputs in its output subfolder (cleared first; default: a temp folder)")]
    public string? OutputPath { get; set; }

    [Option("data", Required = false, HelpText = "A folder whose contents (e.g. config.xml, profiles.xml) are copied to the app data folder")]
    public string? DataPath { get; set; }

    [Option("app", Required = false, HelpText = "On Windows, the app to run: winforms (default) or wpf")]
    public string? App { get; set; }

    [Option('b', "build", Required = false, HelpText = "Build the app first")]
    public bool Build { get; set; }

    [Option("timeout", Required = false, Default = 120, HelpText = "Timeout in seconds")]
    public int Timeout { get; set; }

    [Option("keep-open", Required = false, HelpText = "Leave the app open when the scenario finishes")]
    public bool KeepOpen { get; set; }
}
