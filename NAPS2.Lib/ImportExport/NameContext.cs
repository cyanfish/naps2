namespace NAPS2.ImportExport;

/// <summary>
/// Describes how a name should be computed by <see cref="NameManager"/>. An operation normally creates one context
/// when it starts and reuses it (with <see cref="BatchIndex"/> updated) for every file it saves.
/// </summary>
public record NameContext
{
    /// <summary>
    /// The date and time substituted for date/time placeholders like "$(YYYY)". Using the same value for every file
    /// in an operation ensures they all share the same timestamp.
    /// </summary>
    public required DateTime DateTime { get; init; }

    /// <summary>
    /// How the name is numbered.
    /// </summary>
    public Numbering Numbering { get; init; } = Numbering.Placeholder;

    /// <summary>
    /// The zero-based index of the file within a batch of files. The number in the name is at least BatchIndex + 1,
    /// which avoids re-checking names already used by earlier files in the batch.
    /// </summary>
    public int? BatchIndex { get; init; }

    /// <summary>
    /// The number of files in the batch, used to pad numbers inserted before the extension to a consistent width
    /// (e.g. "scan.01.jpg" through "scan.10.jpg").
    /// </summary>
    public int? BatchCount { get; init; }

    /// <summary>
    /// Whether the name is a file path. If true, numbering skips names that are reserved or that already exist on disk.
    /// If false (e.g. for PDF metadata or email attachment names), the first number is used as is.
    /// </summary>
    public bool IsFilePath { get; init; } = true;

    /// <summary>
    /// Whether existing files should be overwritten rather than skipped by numbering. Reserved names are still skipped.
    /// </summary>
    public bool Overwrite { get; init; }

    public static NameContext FromSaveParams(SaveParams saveParams) => new()
    {
        DateTime = saveParams.DateTime,
        Numbering = saveParams.OpenEndedSequence || saveParams.BatchCount > 1 ? Numbering.Always
            : saveParams.OverwriteMode == OverwriteMode.Never ? Numbering.OnConflict
            : Numbering.Placeholder,
        BatchIndex = saveParams.BatchIndex,
        BatchCount = saveParams.OpenEndedSequence ? null : saveParams.BatchCount,
        Overwrite = saveParams.OverwriteMode == OverwriteMode.Force
    };
}
