using System.Collections.Immutable;

namespace NAPS2.Scan;

public class PerSourceCaps
{
    public static PerSourceCaps UnionAll(ICollection<PerSourceCaps> caps)
    {
        DpiCaps? dpiCaps = null;
        foreach (var dpiValues in caps.Select(x => x.DpiCaps?.Values).WhereNotNull())
        {
            dpiCaps = new DpiCaps
            {
                Values = (dpiCaps?.Values ?? []).Union(dpiValues).OrderBy(x => x).ToImmutableList()
            };
        }
        BitDepthCaps? bitDepthCaps = null;
        foreach (var bd in caps.Select(x => x.BitDepthCaps).WhereNotNull())
        {
            bitDepthCaps = new BitDepthCaps
            {
                SupportsColor = (bitDepthCaps?.SupportsColor ?? false) || bd.SupportsColor,
                SupportsGrayscale = (bitDepthCaps?.SupportsGrayscale ?? false) || bd.SupportsGrayscale,
                SupportsBlackAndWhite = (bitDepthCaps?.SupportsBlackAndWhite ?? false) || bd.SupportsBlackAndWhite,
            };
        }
        PageSizeCaps? pageSizeCaps = null;
        foreach (var area in caps.Select(x => x.PageSizeCaps?.ScanArea).WhereNotNull())
        {
            pageSizeCaps = new PageSizeCaps
            {
                ScanArea = pageSizeCaps?.ScanArea == null
                    ? area
                    : new PageSize(
                        Math.Max(pageSizeCaps.ScanArea.WidthInInches, area.WidthInInches),
                        Math.Max(pageSizeCaps.ScanArea.HeightInInches, area.HeightInInches),
                        PageSizeUnit.Inch)
            };
        }
        return new PerSourceCaps
        {
            DpiCaps = dpiCaps,
            BitDepthCaps = bitDepthCaps,
            PageSizeCaps = pageSizeCaps
        };
    }

    public DpiCaps? DpiCaps { get; init; }

    public BitDepthCaps? BitDepthCaps { get; init; }

    public PageSizeCaps? PageSizeCaps { get; init; }
}