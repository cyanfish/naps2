using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using NAPS2.Ocr;

namespace NAPS2.Scan
{
    /// <summary>
    /// Scan configuration that is separate from the user profile.
    /// This lets scans behave a bit differently in the Batch Scan window, NAPS2.Console, etc.
    /// </summary>
    public class ScanParams
    {
        public bool DetectPatchCodes { get; set; }

        public bool Modal { get; set; } = true;

        public bool NoUI { get; set; }

        public bool NoAutoSave { get; set; }

        public bool NoThumbnails { get; set; }

        public bool SkipPostProcessing { get; set; }

        public bool? DoOcr { get; set; }

        [IgnoreDataMember]
        public OcrParams OcrParams { get; set; }

        [IgnoreDataMember]
        public CancellationToken OcrCancelToken { get; set; }
    }
}
