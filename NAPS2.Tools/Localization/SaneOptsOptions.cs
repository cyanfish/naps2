using CommandLine;
using NAPS2.Tools.Project;

namespace NAPS2.Tools.Localization;

[Verb("saneopts", HelpText = "Update auto-generated C# files with translations for SANE option values")]
public class SaneOptsOptions : OptionsBase
{
}