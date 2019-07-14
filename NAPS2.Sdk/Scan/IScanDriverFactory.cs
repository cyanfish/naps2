namespace NAPS2.Scan
{
    /// <summary>
    /// An interface used to create instances of IScanDriver based on the driver name.
    /// </summary>
    public interface IScanDriverFactory
    {
        IScanDriver Create(string driverName);
    }
}
