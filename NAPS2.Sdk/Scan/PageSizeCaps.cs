namespace NAPS2.Scan;

public class PageSizeCaps
{
    public PageSize? ScanArea { get; init; }

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