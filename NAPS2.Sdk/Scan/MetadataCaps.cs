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
    /// The URI for an icon associated with the device.
    /// </summary>
    public string? IconUri { get; init; }

    /// <summary>
    /// The version of the protocol or driver interface reported for the device.
    /// The meaning and format are specific to the device's <see cref="Driver"/>.
    /// </summary>
    public string? ProtocolVersion { get; init; }
}