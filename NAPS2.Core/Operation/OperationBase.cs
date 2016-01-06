using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Operation
{
    public abstract class OperationBase : IOperation
    {
        public string ProgressTitle { get; protected set; }

        public bool AllowCancel { get; protected set; }

        public OperationStatus Status { get; protected set; }

        public abstract void Cancel();

        public event EventHandler StatusChanged;

        public event EventHandler Finished;

        public event EventHandler<OperationErrorEventArgs> Error;
    }
}