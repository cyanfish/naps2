using NAPS2.Ocr;

namespace NAPS2.Config;

public static class ConfigExtensions
{
    public static OcrParams DefaultOcrParams(this ScopedConfig config)
    {
        if (!config.Get(c => c.EnableOcr))
        {
            return new OcrParams(null, OcrMode.Default, 0);
        }
        return new OcrParams(
            config.Get(c => c.OcrLanguageCode),
            config.Get(c => c.OcrMode),
            config.Get(c => c.OcrTimeoutInSeconds));
    }

    public static TransactionConfigScope<T> BeginTransaction<T>(this ConfigScope<T> scope) where T : new() => 
        new TransactionConfigScope<T>(scope, () => new T());

    public static IConfigProvider<T> Child<TParent, T>(this IConfigProvider<TParent> parentProvider, Func<TParent, T> childSelector) =>
        new ChildConfigProvider<TParent,T>(parentProvider, childSelector);
}