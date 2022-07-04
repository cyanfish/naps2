using CommandLine;

namespace NAPS2.Tools.Localization;

[Verb("language", HelpText = "Update language resource (.resx) files based on the translation (.po) files")]
public class LanguageOptions
{
    [Option('l', "lang", Required = false, HelpText = "Language to update (otherwise all)")]
    public string? LanguageCode { get; set; }
}