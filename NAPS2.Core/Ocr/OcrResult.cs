using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NAPS2.Ocr
{
    public class OcrResult
    {
        public Rectangle PageBounds { get; set; }

        public IEnumerable<OcrResultElement> Elements { get; set; }
    }
}
