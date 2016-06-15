using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NAPS2.Ocr
{
    public interface IOcrEngine
    {
        bool CanProcess(string langCode);
        OcrResult ProcessImage(string imagePath, string langCode);
    }
}