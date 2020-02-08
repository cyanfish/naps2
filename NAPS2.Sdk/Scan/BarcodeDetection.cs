using System;
using NAPS2.Images.Storage;
using ZXing;

namespace NAPS2.Scan
{
    /// <summary>
    /// A wrapper around the ZXing library that detects patch-t and other barcodes.
    /// http://www.alliancegroup.co.uk/patch-codes.htm
    /// </summary>
    public class BarcodeDetection
    {
        private const string PATCH_T_TEXT = "PATCHT";
        
        public static BarcodeDetection NotAttempted = new BarcodeDetection(false, null);
        
        public static BarcodeDetection Detect(IImage image)
        {
            // TODO: Make more generic
            if (!(image is GdiImage gdiImage))
            {
                throw new InvalidOperationException("Patch code detection only supported for GdiStorage");
            }
            var reader = new BarcodeReader();
            return new BarcodeDetection(true, reader.Decode(gdiImage.Bitmap));
        }
        
        private BarcodeDetection(bool isAttempted, Result detectionResult)
        {
            IsAttempted = isAttempted;
            DetectionResult = detectionResult;
        }

        public Result DetectionResult { get; }
        
        public bool IsAttempted { get; }

        public bool IsBarcodePresent => DetectionResult != null;

        public bool IsPatchT => DetectionResult?.Text == PATCH_T_TEXT;
    }
}