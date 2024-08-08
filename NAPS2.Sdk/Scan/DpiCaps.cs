using System.Collections.Immutable;

namespace NAPS2.Scan;

/// <summary>
/// Represents valid values for ScanOptions.Dpi as part of PerSourceCaps.
/// </summary>
public class DpiCaps
{
    private static readonly int[] TargetCommonValues = [0, 100, 150, 200, 300, 400, 600, 800, 1200, 2400, 4800];

    /// <summary>
    /// Creates an instance of DpiCaps that allows values in the specified range.
    /// </summary>
    /// <param name="min">The lowest valid DPI value, inclusive.</param>
    /// <param name="max">The highest valid DPI value, inclusive.</param>
    /// <param name="step">The increment between valid DPI values (must be >0).</param>
    /// <returns></returns>
    public static DpiCaps ForRange(int min, int max, int step)
    {
        if (step <= 0) return new DpiCaps();
        var values = new List<int>();
        for (int i = min; i <= max; i += step)
        {
            values.Add(i);
        }
        return new DpiCaps
        {
            Values = values.ToImmutableList()
        };
    }

    /// <summary>
    /// Allowed values for ScanOptions.Dpi.
    /// </summary>
    public ImmutableList<int>? Values { get; init; }

    /// <summary>
    /// Recommended values for ScanOptions.Dpi to be presented to the user.
    /// </summary>
    public ImmutableList<int>? CommonValues
    {
        get
        {
            if (Values == null) return null;
            var commonValues = new List<int>();
            int j = 0;
            for (int i = 0; i < Values.Count;i++)
            {
                int value = Values[i];
                if (i == Values.Count - 1)
                {
                    commonValues.Add(value);
                    continue;
                }
                bool include = false;
                while (j < TargetCommonValues.Length && TargetCommonValues[j] <= value)
                {
                    include = true;
                    j++;
                }
                if (include)
                {
                    commonValues.Add(value);
                }
            }
            return commonValues.ToImmutableList();
        }
    }
}