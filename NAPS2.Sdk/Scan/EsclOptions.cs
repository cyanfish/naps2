namespace NAPS2.Scan;

public class EsclOptions
{
    /// <summary>
    /// A list of UUIDs for ESCL devices that should be excluded from GetDevices (i.e. shared from the local machine).
    /// </summary>
    public List<string> ExcludeUuids { get; set; } = new();
}