using CommandLine;
using NAPS2.Tools.Localization;
using NAPS2.Tools.Project;

namespace NAPS2.Tools;

public static class Program
{
    public static void Main(string[] args) =>
        Parser.Default.ParseArguments<CleanOptions, BuildOptions, TemplatesOptions, LanguageOptions>(args).MapResult(
            (CleanOptions opts) => CleanCommand.Run(opts),
            (BuildOptions opts) => BuildCommand.Run(opts),
            (TemplatesOptions opts) => TemplatesCommand.Run(opts),
            (LanguageOptions opts) => LanguageCommand.Run(opts),
            errors => 1);
}