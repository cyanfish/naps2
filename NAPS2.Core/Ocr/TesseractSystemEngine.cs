using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.Ocr
{
    public class TesseractSystemEngine : TesseractBaseEngine
    {
        public TesseractSystemEngine(AppConfigManager appConfigManager) : base(appConfigManager)
        {
        }
    }
}
