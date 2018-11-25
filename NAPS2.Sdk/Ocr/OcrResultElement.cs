using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NAPS2.Ocr
{
    public class OcrResultElement
    {
        public Rectangle Bounds { get; set; }

        public string Text { get; set; }
    }
}
