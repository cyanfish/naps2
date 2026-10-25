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
    /// Whether this save is part of an open-ended sequence of saves to the same name (e.g. auto save) rather than a
    /// self-contained batch. Every file is then numbered without zero-padding to the batch size, like a "$(n)"
    /// placeholder, so that later saves continue the sequence.
    /// </summary>
    public bool OpenEndedSequence { get; init; }

    /// <summary>
    /// The date and time used for placeholders. The same value should be used for all files saved together.
    /// </summary>
    public DateTime DateTime { get; init; } = DateTime.Now;
}
