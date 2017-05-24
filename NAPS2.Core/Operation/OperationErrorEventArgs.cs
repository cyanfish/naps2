using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Operation
{
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
