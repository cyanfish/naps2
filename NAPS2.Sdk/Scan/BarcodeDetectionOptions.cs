using ZXing.Common;

namespace NAPS2.Scan;

/// <summary>
/// Options for detecting barcodes using ZXing.
/// </summary>
public class BarcodeDetectionOptions
{
    public bool DetectBarcodes { get; set; }
        
    public bool PatchTOnly { get; set; }
        
    public DecodingOptions? ZXingOptions { get; set; }
}