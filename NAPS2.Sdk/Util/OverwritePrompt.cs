namespace NAPS2.Util;

// TODO: Refactor to Eto and move to NAPS2.EtoForms (or something non-eto and non-winforms if I want operations in the Sdk...)
/// <summary>
/// A base class for objects that can prompt the user to overwrite an existing file.
///
/// Implementors: WinFormsOverwritePrompt, ConsoleOverwritePrompt
/// </summary>
public abstract class OverwritePrompt
{
    /// <summary>
    /// Asks the user if they would like to overwrite the specified file.
    ///
    /// If DialogResult.Cancel is specified, the current operation should be cancelled even if there are other files to write.
    /// </summary>
    /// <param name="path">The path of the file to overwrite.</param>
    /// <returns>Yes, No, or Cancel.</returns>
    public abstract OverwriteResponse ConfirmOverwrite(string path);
}