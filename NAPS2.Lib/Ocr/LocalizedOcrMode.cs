using NAPS2.Scan;

namespace NAPS2.Ocr;

public enum LocalizedOcrMode
{
    [LocalizedDescription(typeof(SettingsResources), "OcrMode_Fast")]
    Fast,
    [LocalizedDescription(typeof(SettingsResources), "OcrMode_FastWithPreProcess")]
    FastWithPreProcess,
    [LocalizedDescription(typeof(SettingsResources), "OcrMode_Best")]
    Best,
    [LocalizedDescription(typeof(SettingsResources), "OcrMode_BestWithPreProcess")]
    BestWithPreProcess,
    Legacy // Deprecated, not mapped to the Sdk OcrMode
}