using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAPS2.Util;

namespace NAPS2.ImportExport
{
    public class ImportParams
    {
        public ImportParams()
        {
            Slice = Slice.Default;
        }

        public Slice Slice { get; set; }

        public bool DetectPatchCodes { get; set; }
    }
}
