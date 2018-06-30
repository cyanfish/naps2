﻿using System.Collections.Generic;
using System.Drawing;

namespace NAPS2.Ocr
{
    public class OcrResult
    {
        public Rectangle PageBounds { get; set; }

        public IEnumerable<OcrResultElement> Elements { get; set; }

        public bool RightToLeft { get; set; }
    }
}