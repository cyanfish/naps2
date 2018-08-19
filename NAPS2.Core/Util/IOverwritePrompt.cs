using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.Util
{
    /// <summary>
    /// A base interface for objects that can prompt the user to overwrite an existing file.
    ///
    /// Implementors: WinFormsOverwritePrompt, ConsoleOverwritePrompt
    /// </summary>
    public interface IOverwritePrompt
    {
        /// <summary>
        /// Asks the user if they would like to overwrite the specified file.
        ///
        /// If DialogResult.Cancel is specified, the current operation should be cancelled even if there are other files to write.
        /// </summary>
        /// <param name="path">The path of the file to overwrite.</param>
        /// <returns>Yes, No, or Cancel.</returns>
        DialogResult ConfirmOverwrite(string path);
    }
}
