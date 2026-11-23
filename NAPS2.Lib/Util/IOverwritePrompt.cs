namespace NAPS2.Util;

/// <summary>
/// A base class for objects that can prompt the user to overwrite an existing file.
/// </summary>
public interface IOverwritePrompt
{
    /// <summary>
    /// Asks the user if they would like to overwrite the specified file.
    ///
    /// If OverwriteResponse.Abort is specified, the current operation should be cancelled even if there are other files to write.
    /// </summary>
    /// <param name="path">The path of the file to overwrite.</param>
    /// <param name="multipleFiles">Whether the operation writes multiple files, so this one can be skipped while still
    /// writing the others.</param>
    /// <returns>Overwrite, Skip, or Abort.</returns>
    public OverwriteResponse ConfirmOverwrite(string path, bool multipleFiles);
}