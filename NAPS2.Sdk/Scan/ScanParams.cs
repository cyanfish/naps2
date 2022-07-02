using System.Runtime.Serialization;
using System.Threading;
using NAPS2.Ocr;

namespace NAPS2.Scan;

/// <summary>
/// Scan configuration that is separate from the user profile.
/// This lets scans behave a bit differently in the Batch Scan window, NAPS2.Console, etc.
/// </summary>
public class ScanParams
{
    public bool DetectPatchT { get; set; }

    public bool Modal { get; set; } = true;

    public bool NoUI { get; set; }

    public bool NoAutoSave { get; set; }

    public int? ThumbnailSize { get; set; }

    public bool SkipPostProcessing { get; set; }

    public OcrParams? OcrParams { get; set; }

    public CancellationToken OcrCancelToken { get; set; }
}