using System.Collections.Immutable;

namespace NAPS2.Scan;

/// <summary>
/// Represents capabilities specific to a single PaperSource as part of ScanCaps.
/// </summary>
public class PerSourceCaps
{
    /// <summary>
    /// Gets an object representing the union of all possible option values allowed by the provided objects.
    /// This can be helpful when presenting the user with a single set of possible options for multiple sources.
    /// </summary>
    public static PerSourceCaps UnionAll(IEnumerable<PerSourceCaps> caps)
    {
        var capsColl = caps as ICollection<PerSourceCaps> ?? caps.ToList();
        DpiCaps? dpiCaps = null;
        foreach (var dpiValues in capsColl.Select(x => x.DpiCaps?.Values).WhereNotNull())
        {
            dpiCaps = new DpiCaps
            {
                Values = (dpiCaps?.Values ?? []).Union(dpiValues).OrderBy(x => x).ToImmutableList()
            };
        }
        BitDepthCaps? bitDepthCaps = null;
        foreach (var bd in capsColl.Select(x => x.BitDepthCaps).WhereNotNull())
        {
            bitDepthCaps = new BitDepthCaps
            {
                SupportsColor = (bitDepthCaps?.SupportsColor ?? false) || bd.SupportsColor,
                SupportsGrayscale = (bitDepthCaps?.SupportsGrayscale ?? false) || bd.SupportsGrayscale,
                SupportsBlackAndWhite = (bitDepthCaps?.SupportsBlackAndWhite ?? false) || bd.SupportsBlackAndWhite,
            };
        }
        PageSizeCaps? pageSizeCaps = null;
        foreach (var area in capsColl.Select(x => x.PageSizeCaps?.ScanArea).WhereNotNull())
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

    /// <summary>
    /// Valid values for ScanOptions.Dpi.
    /// </summary>
    public DpiCaps? DpiCaps { get; init; }

    /// <summary>
    /// Valid values for ScanOptions.BitDepth.
    /// </summary>
    public BitDepthCaps? BitDepthCaps { get; init; }

    /// <summary>
    /// Valid values for ScanOptions.PageSize.
    /// </summary>
    public PageSizeCaps? PageSizeCaps { get; init; }
}