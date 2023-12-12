namespace NAPS2.Scan;

public class EsclOptions
{
    /// <summary>
    /// The maximum time (in ms) to search for ESCL devices when calling GetDevices or at the start of a scan.
    /// </summary>
    public int SearchTimeout { get; set; } = 5000;
}