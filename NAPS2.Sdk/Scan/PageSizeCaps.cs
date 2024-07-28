namespace NAPS2.Scan;

public record PageSizeCaps(
    PageSize ScanAreaSize
)
{
    private PageSizeCaps() : this(new PageSize(0, 0, PageSizeUnit.Inch))
    {
    }
}