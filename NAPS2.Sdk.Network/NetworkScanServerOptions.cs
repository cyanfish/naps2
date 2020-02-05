namespace NAPS2.Remoting.Network
{
    public class NetworkScanServerOptions
    {
        /// <summary>
        /// The name of the server that will be advertised through the discovery process. If not specified, defaults to
        /// Environment.MachineName.
        /// </summary>
        public string? ServerName { get; set; }
        
        /// <summary>
        /// The port the server should run on. If not specified, an unused port will be chosen.
        /// </summary>
        public int? Port { get; set; }
        
        /// <summary>
        /// Whether the discovery process should be enabled. This will allow clients to find available servers on the
        /// network and advertise the server name/ip/port.
        /// </summary>
        public bool AllowDiscovery { get; set; } = true;

        /// <summary>
        /// The port on which the discovery process should run. If not specified, defaults to the standard NAPS2
        /// discovery port (33277). This must match the client discovery port.
        /// </summary>
        public int? DiscoveryPort { get; set; }
    }
}