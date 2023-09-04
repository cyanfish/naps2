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
                Logger.LogDebug("Discovered ESCL Service: {Name}, ipv4 {Ipv4}, ipv6 {IpV6}, port {Port}, uuid {Uuid}",
                    service.ScannerName, service.IpV4, service.IpV6, service.Port, service.Uuid);
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
        var props = new Dictionary<string, string>();
        foreach (var record in args.Message.AdditionalRecords)
        {
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
        if ((ipv4 == null && ipv6 == null) || port == -1 || !http && !https)
        {
            throw new ArgumentException();
        }

        return new EsclService
        {
            IpV4 = ipv4,
            IpV6 = ipv6,
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