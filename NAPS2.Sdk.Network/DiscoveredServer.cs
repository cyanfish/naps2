using System.Net;

namespace NAPS2.Remoting.Network
{
    public class DiscoveredServer
    {
        internal DiscoveredServer(string name, IPEndPoint ipEndPoint)
        {
            Name = name;
            IPEndPoint = ipEndPoint;
        }

        public string Name { get; }
        
        public IPEndPoint IPEndPoint { get; }
    }
}