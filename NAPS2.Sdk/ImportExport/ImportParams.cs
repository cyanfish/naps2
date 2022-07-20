using NAPS2.Scan;

namespace NAPS2.ImportExport;

public class ImportParams
{
    public ImportParams()
    {
        Slice = Slice.Default;
    }

    public string? Password { get; set; }

    public Slice Slice { get; set; }

    public BarcodeDetectionOptions BarcodeDetectionOptions { get; set; } = new BarcodeDetectionOptions();

    public int? ThumbnailSize { get; set; }
}