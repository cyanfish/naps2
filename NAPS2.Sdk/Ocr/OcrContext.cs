using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Ocr
{
    public class OcrContext
    {
        public static OcrContext None => new OcrContext(null, null, null);

        public OcrContext(OcrParams ocrParams)
        {
            Params = ocrParams;
            EngineManager = OcrEngineManager.Default;
            RequestQueue = OcrRequestQueue.Default;
        }

        public OcrContext(OcrParams ocrParams, OcrEngineManager ocrEngineManager, OcrRequestQueue ocrRequestQueue)
        {
            Params = ocrParams;
            EngineManager = ocrEngineManager;
            RequestQueue = ocrRequestQueue;
        }

        public OcrParams Params { get; }

        public OcrEngineManager EngineManager { get; }

        public OcrRequestQueue RequestQueue { get; }
    }
}
