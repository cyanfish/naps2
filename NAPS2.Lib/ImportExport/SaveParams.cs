namespace NAPS2.ImportExport;

/// <summary>
/// Options shared by the save operations for how files are named and how existing files are handled.
/// </summary>
public record SaveParams
{
    public OverwriteMode OverwriteMode { get; init; } = OverwriteMode.Prompt;

    /// <summary>
    /// The total number of files being saved, including by related operations (e.g. one per scan). When more than one,
    /// a single file is numbered too, and numbers are padded to a consistent width.
    /// </summary>
    public int? BatchCount { get; init; }

    /// <summary>
    /// The zero-based index within the batch of the first file saved by this operation, so that numbering continues
    /// from where related operations left off.
    /// </summary>
    public int? BatchIndex { get; init; }

    /// <summary>
    /// The date and time used for placeholders. The same value should be used for all files saved together.
    /// </summary>
    public DateTime DateTime { get; init; } = DateTime.Now;
}
