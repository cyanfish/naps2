namespace NAPS2.Images;

public class PdfRenderSize
{
    public static readonly PdfRenderSize Default = FromDpi(300);

    public static PdfRenderSize FromDpi(float dpi)
    {
        return new PdfRenderSize { Dpi = dpi };
    }

    public static PdfRenderSize FromDimensions(int width, int height)
    {
        return new PdfRenderSize { Width = width, Height = height };
    }

    public static PdfRenderSize FromIndividualPageSizes(IEnumerable<PdfRenderSize> pageSizes)
    {
        var pageSizesArray = pageSizes.ToArray();
        foreach (var pageSize in pageSizesArray)
        {
            if (pageSize.PageSizes != null) throw new ArgumentException("Individual page sizes can't be nested");
        }
        return new PdfRenderSize { PageSizes = pageSizesArray };
    }

    private PdfRenderSize()
    {
    }

    public float? Dpi { get; private init; }

    public int? Width { get; private init; }

    public int? Height { get; private init; }

    public PdfRenderSize[]? PageSizes { get; set; }

    public (int widthInPx, int heightInPx, int xDpi, int yDpi) GetDimensions(
        float widthInInches, float heightInInches, int pageIndex)
    {
        int widthInPx, heightInPx, xDpi, yDpi;
        var renderSize = PageSizes != null && pageIndex < PageSizes.Length ? PageSizes[pageIndex] : this;
        if (renderSize.PageSizes != null)
        {
            throw new InvalidOperationException("Invalid render size");
        }
        if (renderSize.Dpi is { } dpi)
        {
            // Cap the resolution to 10k pixels in each dimension
            dpi = Math.Min(dpi, 10000 / heightInInches);
            dpi = Math.Min(dpi, 10000 / widthInInches);

            widthInPx = (int) Math.Round(widthInInches * dpi);
            heightInPx = (int) Math.Round(heightInInches * dpi);
            xDpi = (int) Math.Round(dpi);
            yDpi = (int) Math.Round(dpi);
        }
        else
        {
            widthInPx = renderSize.Width!.Value;
            heightInPx = renderSize.Height!.Value;
            xDpi = (int) Math.Round(widthInPx / widthInInches * 72);
            yDpi = (int) Math.Round(widthInPx / heightInInches * 72);
        }
        return (widthInPx, heightInPx, xDpi, yDpi);
    }
}