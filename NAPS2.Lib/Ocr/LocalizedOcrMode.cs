using NAPS2.Scan;

namespace NAPS2.Ocr;

public enum LocalizedOcrMode
{
    [LocalizedDescription(typeof(SettingsResources), "OcrMode_Fast")]
    Fast,
    [LocalizedDescription(typeof(SettingsResources), "OcrMode_Best")]
    Best,
    [LocalizedDescription(typeof(SettingsResources), "OcrMode_Legacy")]
    Legacy // Deprecated, not mapped to the Sdk OcrMode
}