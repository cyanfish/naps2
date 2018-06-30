namespace NAPS2.Scan.Stub
{
    public class StubScanDriverFactory : IScanDriverFactory
    {
        public IScanDriver Create(string driverName)
        {
            return new StubScanDriver(driverName);
        }
    }
}