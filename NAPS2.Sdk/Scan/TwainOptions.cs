namespace NAPS2.Scan
{
    public class TwainOptions
    {
        public TwainDsm Dsm { get; set; }

        public TwainAdapter Adapter { get; set; }

        public TwainTransferMode TransferMode { get; set; }

        /// <summary>
        /// Whether to include include devices that start with "WIA-" in GetDeviceList.
        /// Windows makes WIA devices available to TWAIN applications through a translation layer.
        /// By default they are excluded, since NAPS2 supports using WIA devices directly.
        /// </summary>
        public bool IncludeWiaDevices { get; set; }
    }

    public enum TwainAdapter
    {
        NTwain,
        Legacy
    }

    public enum TwainDsm
    {
        New,
        NewX64,
        Old
    }

    public enum TwainTransferMode
    {
        Native,
        Memory
    }
}