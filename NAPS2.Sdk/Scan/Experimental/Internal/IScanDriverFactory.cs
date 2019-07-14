namespace NAPS2.Scan.Experimental.Internal
{
    internal interface IScanDriverFactory
    {
        IScanDriver Create(ScanOptions options);
    }
}
