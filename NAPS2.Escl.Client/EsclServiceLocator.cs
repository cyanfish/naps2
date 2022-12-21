using System.Net;
using Makaretu.Dns;

namespace NAPS2.Escl.Client;

public class EsclServiceLocator
{
    public async Task<List<EsclService>> Locate()
    {
        using var sd = new ServiceDiscovery();
        var locatedServices = new List<EsclService>();
        sd.ServiceInstanceDiscovered += (sender, args) =>
        {
            try
            {
                var service = ParseService(args);
                lock (locatedServices)
                {
                    locatedServices.Add(service);
                }
            }
            catch (Exception)
            {
                // TODO: Log?
            }
        };
        sd.QueryServiceInstances("_uscan._tcp");
        sd.QueryServiceInstances("_uscans._tcp");
        await Task.Delay(2000);
        // TODO: De-duplicate http/https services?
        return locatedServices;

        // var txtVers = props.GetValueOrDefault("txtvers"); // txt record version
        // var adminUrl = props.GetValueOrDefault("adminurl"); // url to scanner config page
        // var esclVersion = props.GetValueOrDefault("Vers"); // escl version e.g. "2.0"
        // var thumbnail = props.GetValueOrDefault("representation"); // url to png or ico 
        // var urlBasePath = props.GetValueOrDefault("rs"); // no leading (or trailing) slash
        // var scannerName = props.GetValueOrDefault("ty"); // human readable
        // var note = props.GetValueOrDefault("note"); // supposed to be "scanner location", e.g. "Copy Room"
        // // Note jpeg is better in that we can get one image at a time, but pdf does allow png quality potentially
        // // Hopefully decent scanners can support png too
        // // Also for the server we can definitely provide NAPS2-generated pdfs, which is kind of a cool idea for e.g. using from mobile
        // var pdl = props.GetValueOrDefault("pdl"); // comma separated mime types supported "application/pdf,image/jpeg" at minimum
        // var uuid = props.GetValueOrDefault("uuid"); // physical device id
        // var colorSpace = props.GetValueOrDefault("cs"); // comma separated capabilites, "color,grayscale,binary"
        // var source = props.GetValueOrDefault("is"); // "platen,adf,camera" platen = flatbed
        // var duplex = props.GetValueOrDefault("duplex"); // "T"rue or "F"alse
        //
    }

    private EsclService ParseService(ServiceInstanceDiscoveryEventArgs args)
    {
        string name = args.ServiceInstanceName.Labels[0];
        string protocol = args.ServiceInstanceName.Labels[1];
        IPAddress? ip = null;
        int port = -1;
        var props = new Dictionary<string, string>();
        foreach (var record in args.Message.AdditionalRecords)
        {
            if (record is ARecord a)
            {
                ip ??= a.Address;
            }
            if (record is AAAARecord aaaa)
            {
                ip = aaaa.Address;
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
        if (ip == null || port == -1 || !http && !https)
        {
            throw new ArgumentException();
        }
        return new EsclService
        {
            // Uuid = props["uuid"],
            // Name = props["ty"],
            Ip = ip,
            Port = port,
            Tls = https,
            RootUrl = props["rs"] // TODO: More props, some required, some optional maybe
        };
    }

    private string? Get(Dictionary<string,string> props, string key)
    {
        return props.TryGetValue(key, out var value) ? value : null;
    }
}