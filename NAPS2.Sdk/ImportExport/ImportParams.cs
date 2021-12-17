using NAPS2.Scan;
using NAPS2.Util;

namespace NAPS2.ImportExport;

public class ImportParams
{
    public ImportParams()
    {
        Slice = Slice.Default;
    }

    public Slice Slice { get; set; }

    public BarcodeDetectionOptions BarcodeDetectionOptions { get; set; } = new BarcodeDetectionOptions();

    public int? ThumbnailSize { get; set; }
}