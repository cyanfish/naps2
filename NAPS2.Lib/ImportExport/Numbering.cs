namespace NAPS2.ImportExport;

/// <summary>
/// How <see cref="NameManager"/> adds a number to a name.
/// </summary>
public enum Numbering
{
    /// <summary>
    /// No numbering. Number placeholders like "$(nnn)" are left in the name so the number can be chosen later
    /// (e.g. when showing a default file name in a save dialog).
    /// </summary>
    None,

    /// <summary>
    /// The name is numbered only if it has a number placeholder.
    /// </summary>
    Placeholder,

    /// <summary>
    /// As with Placeholder, but a name without a placeholder is also numbered (before the extension, e.g. "scan.1.pdf")
    /// if it would otherwise conflict with an existing or reserved file. This is an alternative to prompting the user
    /// to overwrite the file.
    /// </summary>
    OnConflict,

    /// <summary>
    /// The name is always numbered, using the number placeholder if it has one or inserting the number before the
    /// extension otherwise. This is used when saving one of several files.
    /// </summary>
    Always
}
