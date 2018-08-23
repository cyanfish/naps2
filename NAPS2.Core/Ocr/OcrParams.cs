using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Ocr
{
    public class OcrParams
    {
        public OcrParams()
        {
        }

        public OcrParams(string langCode, OcrMode mode)
        {
            LanguageCode = langCode;
            Mode = mode;
        }

        public string LanguageCode { get; set; }

        public OcrMode Mode { get; set; }
    }
}
