using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Operation
{
    /// <summary>
    /// Arguments for the IOperation.Error event.
    /// </summary>
    public class OperationErrorEventArgs : EventArgs
    {
        public OperationErrorEventArgs(string errorMessage, Exception exception)
        {
            ErrorMessage = errorMessage;
            Exception = exception;
        }

        public string ErrorMessage { get; }

        public Exception Exception { get; }
    }
}
