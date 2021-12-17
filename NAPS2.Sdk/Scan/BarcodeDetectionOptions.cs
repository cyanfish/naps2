using ZXing.Common;

namespace NAPS2.Scan;

public class BarcodeDetectionOptions
{
    public bool DetectBarcodes { get; set; }
        
    public bool PatchTOnly { get; set; }
        
    public DecodingOptions? ZXingOptions { get; set; }
}