namespace NAPS2.Scan;

public class SaneOptions
{
    // TODO: Probably move this to top-level ScanOptions (as e.g. twain might have extra options too)
    public KeyValueScanOptions? KeyValueOptions { get; set; }
}