namespace NAPS2.Scan.Internal
{
    internal interface IScanDriverFactory
    {
        IScanDriver Create(ScanOptions options);
    }
}
