namespace NAPS2.ImportExport;

/// <summary>
/// How a save operation handles existing files.
/// </summary>
public enum OverwriteMode
{
    /// <summary>
    /// Ask the user before overwriting an existing file. Auto-numbered files skip existing files instead.
    /// </summary>
    Prompt,

    /// <summary>
    /// The user has already confirmed (e.g. in a save dialog) that the file can be overwritten, so there is no prompt.
    /// Auto-numbered files still skip existing files.
    /// </summary>
    Confirmed,

    /// <summary>
    /// Never overwrite or prompt; if the file exists, a number is added to the file name instead.
    /// </summary>
    Never,

    /// <summary>
    /// Overwrite existing files without asking, including auto-numbered files.
    /// </summary>
    Force
}

public static class OverwriteModeExtensions
{
    /// <summary>
    /// Whether the user should be asked before saving to the given path.
    /// </summary>
    public static bool ShouldPromptForPath(this OverwriteMode overwriteMode, string path) =>
        overwriteMode == OverwriteMode.Prompt && File.Exists(path);
}
