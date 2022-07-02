using NAPS2.Ocr;

namespace NAPS2.Config;

public static class ConfigExtensions
{
    public static OcrParams DefaultOcrParams(this Naps2Config config)
    {
        if (!config.Get(c => c.EnableOcr))
        {
            return OcrParams.Empty;
        }
        return new OcrParams(
            config.Get(c => c.OcrLanguageCode),
            config.Get(c => c.OcrMode),
            config.Get(c => c.OcrTimeoutInSeconds));
    }

    public static OcrParams OcrAfterScanningParams(this Naps2Config config)
    {
        if (!config.Get(c => c.OcrAfterScanning))
        {
            return OcrParams.Empty;
        }
        return config.DefaultOcrParams();
    }
}