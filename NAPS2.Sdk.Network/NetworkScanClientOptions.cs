namespace NAPS2.Remoting.Network
{
    public class NetworkScanClientOptions
    {
        /// <summary>
        /// The port on which the discovery process should run. If not specified, defaults to the standard NAPS2
        /// discovery port (33277). This must match the server discovery port.
        /// </summary>
        public int? DiscoveryPort { get; set; }

        /// <summary>
        /// Whether to request the server sends scan progress events. This may result in many packets being sent, so
        /// consider setting to false for poor network conditions.
        /// </summary>
        public bool RequestProgress { get; set; } = true;
    }
}