using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Operation
{
    /// <summary>
    /// A representation of the current status of an IOperation.
    /// </summary>
    public class OperationStatus
    {
        public string StatusText { get; set; }

        public int CurrentProgress { get; set; }

        public int MaxProgress { get; set; }

        public bool IndeterminateProgress { get; set; }

        public OperationProgressType ProgressType { get; set; }
    }
}
