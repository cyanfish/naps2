using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Operation
{
    /// <summary>
    /// A base interface for objects capabable of displaying progress for an operation.
    ///
    /// Implementors: WinFormsOperationProgress, ConsoleOperationProgress
    /// </summary>
    public interface IOperationProgress
    {
        void ShowProgress(IOperation op);
    }
}
