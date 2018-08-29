using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan
{
    /// <summary>
    /// Scan configuration that is separate from the user profile.
    /// This lets scans behave a bit differently in the Batch Scan window, NAPS2.Console, etc.
    /// </summary>
    public class ScanParams
    {
        public bool DetectPatchCodes { get; set; }

        public bool NoUI { get; set; }

        public bool NoAutoSave { get; set; }
    }
}
