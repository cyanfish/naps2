using CommandLine;
using NAPS2.Tools.Project;

namespace NAPS2.Tools.Localization;

[Verb("pullpo", HelpText = "Download .po files from Crowdin")]
public class PullTranslationsOptions : OptionsBase
{
}