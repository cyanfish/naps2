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

    /// <summary>
    /// Creates a transaction wrapping the provided scope. See TransactionConfigScope for more documentation.
    /// </summary>
    public static TransactionConfigScope<T> BeginTransaction<T>(this ConfigScope<T> scope) => new(scope);
}