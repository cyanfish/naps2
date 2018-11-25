using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Scan.Images;

namespace NAPS2.Ocr
{
    public class OcrRequest
    {
        public OcrRequest(OcrRequestParams reqParams)
        {
            Params = reqParams;
        }

        public OcrRequestParams Params { get; }

        public string TempImageFilePath { get; set; }

        public CancellationTokenSource CancelSource { get; } = new CancellationTokenSource();

        public ManualResetEvent WaitHandle { get; } = new ManualResetEvent(false);

        public bool IsProcessing { get; set; }

        public OcrResult Result { get; set; }

        public int ForegroundCount { get; set; }

        public int BackgroundCount { get; set; }
    }
}
