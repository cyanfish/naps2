using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Util
{
    /// <summary>
    /// A base interface for objects capable of displaying error output.
    ///
    /// Implementors: MessageBoxErrorOutput, ConsoleErrorOutput
    /// </summary>
    public interface IErrorOutput
    {
        void DisplayError(string errorMessage);

        void DisplayError(string errorMessage, string details);

        void DisplayError(string errorMessage, Exception exception);
    }
}
