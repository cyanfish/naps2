namespace NAPS2.Scan
{
    /// <summary>
    /// The representation of a scanning device identified by a driver.
    /// </summary>
    public class ScanDevice
    {
        public ScanDevice(string? id, string? name)
        {
            ID = id;
            Name = name;
        }

        public ScanDevice()
        {
        }

        public string? ID { get; set; }

        public string? Name { get; set; }
    }
}
