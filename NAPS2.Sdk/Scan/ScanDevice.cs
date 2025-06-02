namespace NAPS2.Scan;

/// <summary>
/// The representation of a scanning device and its corresponding driver.
/// </summary>
public sealed record ScanDevice(
    Driver Driver,
    string ID,
    string Name,
    string? IconUri = null,
    string? ConnectionUri = null)
{
    private ScanDevice() : this(Driver.Default, "", "")
    {
    }

    public bool Equals(ScanDevice? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Driver == other.Driver && ID == other.ID && Name == other.Name;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (int) Driver;
            hashCode = (hashCode * 397) ^ ID.GetHashCode();
            hashCode = (hashCode * 397) ^ Name.GetHashCode();
            return hashCode;
        }
    }
}