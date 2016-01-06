using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Operation
{
    public class OperationErrorEventArgs : EventArgs
    {
        public OperationErrorEventArgs(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; private set; }
    }
}
