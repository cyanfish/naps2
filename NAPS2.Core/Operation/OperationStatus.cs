using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Operation
{
    public class OperationStatus
    {
        public string StatusText { get; set; }

        public int CurrentProgress { get; set; }

        public int MaxProgress { get; set; }

        public bool IndeterminateProgress { get; set; }

        public bool Success { get; set; }
    }
}
