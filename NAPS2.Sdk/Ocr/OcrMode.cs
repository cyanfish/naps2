namespace NAPS2.Ocr;

/// <summary>
/// The mode of an OCR request (fast/best), if supported by the engine.
/// </summary>
[Flags]
public enum OcrMode
{
    Default = 0,
    Fast = 1,
    Best = 2,
    WithPreProcess = 4,
    FastWithPreProcess = Fast | WithPreProcess,
    BestWithPreProcess = Best | WithPreProcess
}