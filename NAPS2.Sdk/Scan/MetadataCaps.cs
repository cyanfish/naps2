namespace NAPS2.Scan;

/// <summary>
/// Represents scanner metadata as part of ScanCaps.
/// </summary>
public class MetadataCaps
{
    /// <summary>
    /// For SANE, this is the backend name.
    /// </summary>
    public string? DriverSubtype { get; init; }

    /// <summary>
    /// The device manufacturer.
    /// </summary>
    public string? Manufacturer { get; init; }

    /// <summary>
    /// The device model name.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// The device serial number.
    /// </summary>
    public string? SerialNumber { get; init; }

    /// <summary>
    /// The location note associated with the device.
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// The URI for an icon associated with the device.
    /// </summary>
    public string? IconUri { get; init; }
}