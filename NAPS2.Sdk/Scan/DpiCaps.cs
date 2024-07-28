using System.Collections.Immutable;

namespace NAPS2.Scan;

public class DpiCaps
{
    public ImmutableList<int>? Values { get; init; }

    public int Min { get; init; }

    public int Max { get; init; }

    public int Step { get; init; }
}