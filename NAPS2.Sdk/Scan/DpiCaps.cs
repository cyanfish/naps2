using System.Collections.Immutable;

namespace NAPS2.Scan;

public record DpiCaps(
    ImmutableList<int>? Values,
    int Min,
    int Max,
    int Step
);