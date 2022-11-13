using NAPS2.Tools.Project;

namespace NAPS2.Tools.Localization;

public class LanguageCommand : ICommand<LanguageOptions>
{
    public int Run(LanguageOptions opts)
    {
        // TODO: Handle null langCode to detect all languages
        var langCode = opts.LanguageCode;
        var ctx = new LanguageContext(langCode ?? throw new ArgumentNullException());
        ctx.Load(Path.Combine(Paths.SolutionRoot, "NAPS2.Lib", "Lang", "po", $"{langCode}.po"));
        ctx.Translate(Path.Combine(Paths.SolutionRoot, "NAPS2.Lib", "Lang", "Resources"), false);
        return 0;
    }
}