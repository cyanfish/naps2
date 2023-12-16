using System.Net;
using Makaretu.Dns;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NAPS2.Escl.Client;

public class EsclServiceLocator : IDisposable
{
    private readonly ServiceDiscovery _discovery;
    private readonly HashSet<ServiceKey> _locatedServices = new();
    private bool _started;
    private int _nextQueryInterval = 1000;

    public EsclServiceLocator(Action<EsclService> serviceCallback)
    {
        _discovery = new ServiceDiscovery();
        _discovery.ServiceInstanceDiscovered += (_, args) =>
        {
            try
            {
                var service = ParseService(args);
                var serviceKey = new ServiceKey(service.ScannerName, service.Uuid, service.Port, service.IpV4, service.IpV6);
                if (!_locatedServices.Add(serviceKey))
                {
                    // Don't callback for duplicates
                    return;
                }
                Logger.LogDebug("Discovered ESCL Service: {Name}, instance {Instance}, endpoint {Endpoint}, ipv4 {Ipv4}, ipv6 {IpV6}, host {Host}, port {Port}, uuid {Uuid}",
                    service.ScannerName, args.ServiceInstanceName, args.RemoteEndPoint, service.IpV4, service.IpV6, service.Host, service.Port, service.Uuid);
                serviceCallback(service);
            }
            catch (Exception)
            {
                // TODO: Log?
            }
        };
    }

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public void Start()
    {
        if (_started) throw new InvalidOperationException("Already started");
        _started = true;

        Query();
    }

    private void Query()
    {
        if (_discovery.Mdns == null)
        {
            return;
        }
        // TODO: De-duplicate http/https services?
        _discovery.QueryServiceInstances("_uscan._tcp");
        _discovery.QueryServiceInstances("_uscans._tcp");

        // We query once when we start, then again after 1s, 2s, etc. to account for race conditions where there was a
        // previous query/answer on the network just before we started listening, which would prevent us from receiving
        // a response. See the following:
        //
        // "When retransmitting Multicast DNS queries to implement continuous monitoring, the interval between the first
        // two queries MUST be at least one second, and the intervals between successive queries MUST increase by at
        // least a factor of two."
        // https://datatracker.ietf.org/doc/html/rfc6762#section-5.2
        //
        // "A Multicast DNS responder MUST NOT multicast a record on a given interface until at least one second has
        // elapsed since the last time that record was multicast on that particular interface."
        // https://datatracker.ietf.org/doc/html/rfc6762#section-6
        Task.Delay(_nextQueryInterval).ContinueWith(_ => Query());
        _nextQueryInterval *= 2;
    }

    private EsclService ParseService(ServiceInstanceDiscoveryEventArgs args)
    {
        string name = args.ServiceInstanceName.Labels[0];
        bool isTls = false;
        IPAddress? ipv4 = null, ipv6 = null;
        int port = -1;
        string? host = null;
        var props = new Dictionary<string, string>();
        foreach (var record in args.Message.AdditionalRecords)
        {
            Logger.LogTrace("{Type} {Record}", record.GetType().Name, record);
            if (record is ARecord a)
            {
                ipv4 = a.Address;
            }
            if (record is AAAARecord aaaa)
            {
                ipv6 = aaaa.Address;
            }
            if (record is SRVRecord srv)
            {
                bool recordIsTls = srv.Name.IsSubdomainOf(DomainName.Join("_uscans", "_tcp", "local"));
                if (host == null || recordIsTls)
                {
                    // HTTPS overrides HTTP but not the other way around
                    port = srv.Port;
                    host = srv.Target.ToString();
                    isTls = recordIsTls;
                }
            }
            if (record is TXTRecord txt)
            {
                foreach (var str in txt.Strings)
                {
                    var eq = str.IndexOf("=", StringComparison.Ordinal);
                    if (eq != -1)
                    {
                        props[str.Substring(0, eq).ToLowerInvariant()] = str.Substring(eq + 1);
                    }
                }
            }
        }
        if ((ipv4 == null && ipv6 == null) || port == -1 || host == null)
        {
            throw new ArgumentException();
        }

        return new EsclService
        {
            IpV4 = ipv4,
            IpV6 = ipv6,
            Host = host,
            RemoteEndpoint = args.RemoteEndPoint.Address,
            Port = port,
            Tls = isTls,
            ScannerName = props["ty"],
            RootUrl = props["rs"],
            TxtVersion = Get(props, "txtvers"),
            AdminUrl = Get(props, "adminurl"),
            EsclVersion = Get(props, "Vers"),
            Thumbnail = Get(props, "representation"),
            Note = Get(props, "note"),
            MimeTypes = Get(props, "pdl")?.Split(','),
            Uuid = Get(props, "uuid"),
            ColorOptions = Get(props, "cs")?.Split(','),
            SourceOptions = Get(props, "is"),
            DuplexSupported = Get(props, "duplex")?.ToUpperInvariant() == "T"
        };
    }

    private string? Get(Dictionary<string, string> props, string key)
    {
        return props.TryGetValue(key, out var value) ? value : null;
    }

    public void Dispose()
    {
        _discovery.Dispose();
    }

    private record ServiceKey(string? ScannerName, string? Uuid, int Port, IPAddress? IpV4, IPAddress? IpV6);
}
