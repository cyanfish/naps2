using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ZXing;

namespace NAPS2.Scan
{
    /// <summary>
    /// A wrapper around the ZXing library that detects patch codes.
    /// http://www.alliancegroup.co.uk/patch-codes.htm
    /// </summary>
    public class PatchCodeDetector
    {
        public static PatchCode Detect(Bitmap bitmap)
        {
            IBarcodeReader reader = new BarcodeReader();
            var barcodeResult = reader.Decode(bitmap);
            if (barcodeResult != null)
            {
                switch (barcodeResult.Text)
                {
                    case "PATCH1":
                        return PatchCode.Patch1;
                    case "PATCH2":
                        return PatchCode.Patch2;
                    case "PATCH3":
                        return PatchCode.Patch3;
                    case "PATCH4":
                        return PatchCode.Patch4;
                    case "PATCH6":
                        return PatchCode.Patch6;
                    case "PATCHT":
                        return PatchCode.PatchT;
                }
            }
            return PatchCode.None;
        }
    }
}
