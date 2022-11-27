using NAPS2.Scan;

namespace NAPS2.Ocr;

public enum LocalizedOcrMode
{
    [LocalizedDescription(typeof(SettingsResources), "OcrMode_Fast")]
    Fast,
    [LocalizedDescription(typeof(SettingsResources), "OcrMode_Best")]
    Best,
    Legacy // Deprecated, not mapped to the Sdk OcrMode
}