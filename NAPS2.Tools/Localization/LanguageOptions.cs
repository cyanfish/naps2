using CommandLine;
using NAPS2.Tools.Project;

namespace NAPS2.Tools.Localization;

[Verb("language", HelpText = "Update language resource (.resx) files based on the translation (.po) files")]
public class LanguageOptions : OptionsBase
{
    [Option('l', "lang", Required = false, HelpText = "Language to update (otherwise all)")]
    public string? LanguageCode { get; set; }
}