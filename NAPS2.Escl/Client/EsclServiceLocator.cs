using System.Net;
using Makaretu.Dns;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NAPS2.Escl.Client;

public class EsclServiceLocator : IDisposable
{
    private readonly ServiceDiscovery _discovery;
    private bool _started;

    public EsclServiceLocator(Action<EsclService> serviceCallback)
    {
        _discovery = new ServiceDiscovery();
        _discovery.ServiceInstanceDiscovered += (_, args) =>
        {
            try
            {
                var service = ParseService(args);
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


        // We query once when we start, then once again after 1s to account for race conditions where there was a
        // previous query/answer on the network just before we started listening, which would prevent us from receiving
        // a response. See the following:
        //
        // "When retransmitting Multicast DNS queries to implement continuous monitoring, the interval between the first
        // two queries MUST be at least one second."
        // https://datatracker.ietf.org/doc/html/rfc6762#section-5.2
        //
        // "A Multicast DNS responder MUST NOT multicast a record on a given interface until at least one second has
        // elapsed since the last time that record was multicast on that particular interface."
        // https://datatracker.ietf.org/doc/html/rfc6762#section-6
        Query();
        Task.Delay(1000).ContinueWith(_ =>
        {
            if (_discovery.Mdns != null)
            {
                Query();
            }
        });
    }

    private void Query()
    {
        // TODO: De-duplicate http/https services?
        _discovery.QueryServiceInstances("_uscan._tcp");
        _discovery.QueryServiceInstances("_uscans._tcp");
    }

    private EsclService ParseService(ServiceInstanceDiscoveryEventArgs args)
    {
        string name = args.ServiceInstanceName.Labels[0];
        string protocol = args.ServiceInstanceName.Labels[1];
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
                port = srv.Port;
                host = srv.Target.ToString();
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
        bool http = protocol == "_uscan";
        bool https = protocol == "_uscans";
        if ((ipv4 == null && ipv6 == null) || port == -1 || host == null || !http && !https)
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
            Tls = https,
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
}