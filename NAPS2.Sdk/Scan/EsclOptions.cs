using NAPS2.Escl;

namespace NAPS2.Scan;

/// <summary>
/// Scanning options specific to the ESCL driver.
/// </summary>
public class EsclOptions
{
    /// <summary>
    /// The maximum time (in ms) to search for ESCL devices when calling GetDevices or at the start of a scan.
    /// </summary>
    public int SearchTimeout { get; set; } = 5000;

    /// <summary>
    /// The security policy to use for ESCL connections.
    /// </summary>
    public EsclSecurityPolicy SecurityPolicy { get; set; }
}