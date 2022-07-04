using CommandLine;
using NAPS2.Tools.Localization;
using NAPS2.Tools.Project;

namespace NAPS2.Tools;

public static class Program
{
    public static void Main(string[] args) =>
        Parser.Default.ParseArguments<CleanOptions, TemplatesOptions, LanguageOptions>(args).MapResult(
            (CleanOptions opts) => CleanCommand.Run(),
            (TemplatesOptions opts) => TemplatesCommand.Run(),
            (LanguageOptions opts) => LanguageCommand.Run(opts.LanguageCode),
            errors => 1);
}