namespace NAPS2.Util;

/// <summary>
/// A base class for objects that can prompt the user to overwrite an existing file.
/// </summary>
public interface IOverwritePrompt
{
    /// <summary>
    /// Asks the user if they would like to overwrite the specified file.
    ///
    /// If OverwriteResponse.Cancel is specified, the current operation should be cancelled even if there are other files to write.
    /// </summary>
    /// <param name="path">The path of the file to overwrite.</param>
    /// <returns>Yes, No, or Cancel.</returns>
    public OverwriteResponse ConfirmOverwrite(string path);
}