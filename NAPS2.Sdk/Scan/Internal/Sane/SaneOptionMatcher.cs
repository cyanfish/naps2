namespace NAPS2.Scan.Internal.Sane;

public class SaneOptionMatcher
{
    private readonly HashSet<string> _knownValues;
    private readonly string[] _substrings;
    private readonly List<SaneOptionMatcher> _excludes = new();

    public SaneOptionMatcher(IEnumerable<string> knownValues, params string[] substrings)
    {
        _knownValues = knownValues.ToHashSet();
        _substrings = substrings;
    }

    public SaneOptionMatcher Exclude(SaneOptionMatcher other)
    {
        _excludes.Add(other);
        return this;
    }

    public bool Matches(string value)
    {
        return !_excludes.Any(x => x.Matches(value)) &&
               (_knownValues.Contains(value) || _substrings.Any(value.ContainsInvariantIgnoreCase));
    }
}