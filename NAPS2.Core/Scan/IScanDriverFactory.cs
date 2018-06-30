namespace NAPS2.Scan
{
    public interface IScanDriverFactory
    {
        IScanDriver Create(string driverName);
    }
}