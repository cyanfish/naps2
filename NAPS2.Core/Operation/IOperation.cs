using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NAPS2.Operation
{
    /// <summary>
    /// A representation of a long-running operation with progress reporting and cancellation.
    /// </summary>
    public interface IOperation
    {
        string ProgressTitle { get; }

        bool AllowCancel { get; }

        bool AllowBackground { get; }

        OperationStatus Status { get; }

        Task<bool> Success { get; }

        void Cancel();

        event EventHandler StatusChanged;

        event EventHandler Finished;

        event EventHandler<OperationErrorEventArgs> Error;
    }
}
