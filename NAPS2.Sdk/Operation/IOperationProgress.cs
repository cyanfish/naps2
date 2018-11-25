using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.Operation
{
    /// <summary>
    /// A base interface for objects capabable of displaying progress for an operation.
    ///
    /// Implementors: WinFormsOperationProgress, ConsoleOperationProgress
    /// </summary>
    public interface IOperationProgress
    {
        void Attach(IOperation op);

        void ShowProgress(IOperation op);

        void ShowModalProgress(IOperation op);

        void ShowBackgroundProgress(IOperation op);

        void RenderStatus(IOperation op, Label textLabel, Label numberLabel, ProgressBar progressBar);

        List<IOperation> ActiveOperations { get; }
    }
}
