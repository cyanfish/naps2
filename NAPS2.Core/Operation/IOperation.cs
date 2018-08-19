using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Operation
{
    /// <summary>
    /// A representation of a long-running operation with progress reporting and cancellation.
    /// </summary>
    public interface IOperation
    {
        string ProgressTitle { get; }

        bool AllowCancel { get; }

        OperationStatus Status { get; }

        void Cancel();

        void WaitUntilFinished();

        event EventHandler StatusChanged;

        event EventHandler Finished;

        event EventHandler<OperationErrorEventArgs> Error;
    }
}
