namespace NAPS2.Scan;

/// <summary>
/// Represents valid values for ScanOptions.PageSize as part of PerSourceCaps.
/// </summary>
public class PageSizeCaps
{
    /// <summary>
    /// Gets the size of the full scan area (i.e. maximum page size).
    /// </summary>
    public PageSize? ScanArea { get; init; }

    /// <summary>
    /// Determines whether the provided page size fits within the full scan area (with 1% margin of error).
    /// </summary>
    public bool Fits(PageSize pageSize)
    {
        if (ScanArea == null)
        {
            // If no scan area is specified, we consider it as unbounded
            return true;
        }
        // Allow 1% margin
        return pageSize.WidthInInches <= ScanArea.WidthInInches * 1.01m &&
               pageSize.HeightInInches <= ScanArea.HeightInInches * 1.01m;
    }
}