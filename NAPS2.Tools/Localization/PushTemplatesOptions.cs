using CommandLine;
using NAPS2.Tools.Project;

namespace NAPS2.Tools.Localization;

[Verb("pushpot", HelpText = "Upload templates.pot to Crowdin")]
public class PushTemplatesOptions : OptionsBase
{
}