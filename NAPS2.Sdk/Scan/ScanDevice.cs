using System.Diagnostics.CodeAnalysis;

namespace NAPS2.Scan;

/// <summary>
/// The representation of a scanning device identified by a driver.
/// </summary>
public class ScanDevice
{
    // Need an empty constructor for XML compatibility
    public ScanDevice()
    {
    }

    [SetsRequiredMembers]
    public ScanDevice(string id, string name)
    {
        ID = id;
        Name = name;
    }


    // Keeping old naming scheme for XML compatibility
    // ReSharper disable once InconsistentNaming
    public required string ID { get; init; }
    public required string Name { get; init; }
}