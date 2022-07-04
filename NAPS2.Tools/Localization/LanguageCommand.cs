namespace NAPS2.Tools.Localization;

public static class LanguageCommand
{
    public static int Run(string? langCode)
    {
        // TODO: Handle null langCode to detect all languages
        var ctx = new LanguageContext(langCode ?? throw new ArgumentNullException());
        ctx.Load(Path.Combine(Paths.Root, $@"NAPS2.Core\Lang\po\{langCode}.po"));
        ctx.Translate(Path.Combine(Paths.Root, @"NAPS2.Core\Lang\Resources"), false);
        ctx.Translate(Path.Combine(Paths.Root, @"NAPS2.Core\WinForms"), true);
        return 0;
    }
}