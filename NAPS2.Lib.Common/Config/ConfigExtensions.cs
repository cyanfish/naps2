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
            MapOcrMode(config.Get(c => c.OcrMode)),
            config.Get(c => c.OcrTimeoutInSeconds));
    }

    private static OcrMode MapOcrMode(LocalizedOcrMode ocrMode)
    {
        switch (ocrMode)
        {
            case LocalizedOcrMode.Fast:
                return OcrMode.Fast;
            case LocalizedOcrMode.Best:
                return OcrMode.Best;
            default:
                return OcrMode.Default;
        }
    }

    public static OcrParams OcrAfterScanningParams(this Naps2Config config)
    {
        if (!config.Get(c => c.OcrAfterScanning))
        {
            return OcrParams.Empty;
        }
        return config.DefaultOcrParams();
    }

    public static int ThumbnailSize(this Naps2Config config)
    {
        var size = config.Get(c => c.ThumbnailSize);
        if (size == 0)
        {
            return ThumbnailSizes.DEFAULT_SIZE;
        }
        return ThumbnailSizes.Validate(size);
    }
}