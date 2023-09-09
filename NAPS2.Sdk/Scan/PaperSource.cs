namespace NAPS2.Scan;

/// <summary>
/// Specifies the physical paper source for devices that have multiple options (Flatbed, Feeder, Duplex).
/// </summary>
public enum PaperSource
{
    /// <summary>
    /// Use a supported paper source for the device. Generally this prioritizes Flatbed -> Feeder -> Duplex, but it
    /// depends on the driver. This may choose between Feeder and Flatbed based on whether there is paper in the feeder.
    /// </summary>
    Auto,

    /// <summary>
    /// Use the flatbed component of the scanner to scan a single page.
    /// </summary>
    Flatbed,

    /// <summary>
    /// Use the automatic document feeder component of the scanner, potentially scanning multiple pages.
    /// </summary>
    Feeder,

    /// <summary>
    /// Use the automatic document feeder component of the scanner with double-sided scanning, potentially scanning
    /// multiple pages.
    /// </summary>
    Duplex
}