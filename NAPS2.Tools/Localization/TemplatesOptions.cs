using CommandLine;
using NAPS2.Tools.Project;

namespace NAPS2.Tools.Localization;

[Verb("templates", HelpText = "Update templates (.pot) files based on project resources (.resx) files")]
public class TemplatesOptions : OptionsBase
{
}