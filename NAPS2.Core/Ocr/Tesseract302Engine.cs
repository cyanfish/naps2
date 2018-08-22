using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.Ocr
{
    public class Tesseract302Engine : TesseractBaseEngine
    {
        public Tesseract302Engine(AppConfigManager appConfigManager) : base(appConfigManager)
        {
        }
    }
}
