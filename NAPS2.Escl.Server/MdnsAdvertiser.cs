using Makaretu.Dns;
using Makaretu.Dns.Resolving;

namespace NAPS2.Escl.Server;

public class MdnsAdvertiser : IDisposable
{
    private readonly Dictionary<string, ServiceProfile> _serviceProfiles = new();
    private readonly Dictionary<string, ServiceProfile> _serviceProfiles2 = new();

    // Initializing ServiceDiscovery is slow (it queries network interfaces) so use lazily
    private readonly Lazy<ServiceDiscovery> _sd = new(() => new ServiceDiscovery());

    private ServiceDiscovery ServiceDiscovery => _sd.Value;

    public void AdvertiseDevice(EsclDeviceConfig deviceConfig, bool hasHttp, bool hasHttps)
    {
        var caps = deviceConfig.Capabilities;
        if (caps.Uuid == null)
        {
            throw new ArgumentException("UUID must be specified");
        }
        if (!hasHttp && !hasHttps)
        {
            return;
        }
        var name = caps.MakeAndModel;

        // HTTP+HTTPS should be handled by responding with the relevant records for both _uscan and _uscans when either
        // is queried. This isn't handled out-of-the-box by the MDNS library so we need to do some extra work.
        var httpProfile = new ServiceProfile(name, "_uscan._tcp", (ushort) deviceConfig.Port);
        var httpsProfile = new ServiceProfile(name, "_uscans._tcp", (ushort) deviceConfig.TlsPort);
        // If only one of HTTP or HTTPS is enabled, then we use that as the service. If both are enabled, we use the
        // HTTP service as a baseline and then hack in the HTTPS records later.
        var service = hasHttp ? httpProfile : httpsProfile;

        var domain = $"naps2-{caps.Uuid}";
        var hostName = DomainName.Join(domain, service.Domain);

        // Replace the default TXT record with the first TXT record (HTTP if used, HTTPS otherwise)
        service.Resources.RemoveAll(x => x is TXTRecord);
        service.Resources.Add(CreateTxtRecord(deviceConfig, hasHttp, service, caps, name));

        // NSEC records are recommended by RFC6762 to annotate that there's no more info for this host
        service.Resources.Add(new NSECRecord
            { Name = hostName, NextOwnerName = hostName, Types = [DnsType.A, DnsType.AAAA] });

        if (hasHttp && hasHttps)
        {
            // If both HTTP and HTTPS are enabled, we add the extra HTTPS records here
            service.Resources.Add(new PTRRecord
            {
                Name = httpsProfile.QualifiedServiceName,
                DomainName = httpsProfile.FullyQualifiedName
            });
            service.Resources.Add(new SRVRecord
            {
                Name = httpsProfile.FullyQualifiedName,
                Port = (ushort) deviceConfig.TlsPort
            });
            service.Resources.Add(CreateTxtRecord(deviceConfig, false, httpsProfile, caps, name));
        }

        // The default HostName isn't correct, it should be "naps2-uuid.local" (the actual host) instead of
        // "name._uscan.local" (the service name)
        service.HostName = hostName;

        // Send the full set of HTTP/HTTPS records to anyone currently listening
        ServiceDiscovery.Announce(service);

        // Set up to respond to _uscan/_uscans queries with our records.
        ServiceDiscovery.Advertise(service);
        if (hasHttp && hasHttps)
        {
            // Add _uscans to the available services (_uscan was already mapped in Advertise())
            ServiceDiscovery.NameServer.Catalog[ServiceDiscovery.ServiceName].Resources.Add(new PTRRecord
                { Name = ServiceDiscovery.ServiceName, DomainName = httpsProfile.QualifiedServiceName });
            // Cross-reference _uscan to the HTTPS records
            ServiceDiscovery.NameServer.Catalog[httpProfile.QualifiedServiceName].Resources.Add(new PTRRecord
                { Name = httpsProfile.QualifiedServiceName, DomainName = httpsProfile.FullyQualifiedName });
            // Add a _uscans reference with both HTTP and HTTPS records
            ServiceDiscovery.NameServer.Catalog[httpsProfile.QualifiedServiceName] = new Node
            {
                Name = httpsProfile.QualifiedServiceName, Authoritative = true, Resources =
                {
                    new PTRRecord
                        { Name = httpProfile.QualifiedServiceName, DomainName = httpProfile.FullyQualifiedName },
                    new PTRRecord
                        { Name = httpsProfile.QualifiedServiceName, DomainName = httpsProfile.FullyQualifiedName }
                }
            };
        }

        // Persist the profiles so they can be unadvertised later
        _serviceProfiles.Add(caps.Uuid, service);
        if (hasHttp && hasHttps)
        {
            _serviceProfiles2.Add(caps.Uuid, httpsProfile);
        }
    }

    private static TXTRecord CreateTxtRecord(EsclDeviceConfig deviceConfig, bool http, ServiceProfile service,
        EsclCapabilities caps, string? name)
    {
        var record = new TXTRecord();
        record.Name = service.FullyQualifiedName;
        record.Strings.Add("txtvers=1");
        record.Strings.Add("Vers=2.0"); // TODO: verify
        if (deviceConfig.Capabilities.IconPng != null)
        {
            record.Strings.Add(
                http
                    ? $"representation=http://naps2-{caps.Uuid}.local.:{deviceConfig.Port}/eSCL/icon.png"
                    : $"representation=https://naps2-{caps.Uuid}.local.:{deviceConfig.TlsPort}/eSCL/icon.png");
        }
        record.Strings.Add("rs=eSCL");
        record.Strings.Add($"ty={name}");
        record.Strings.Add("pdl=application/pdf,image/jpeg,image/png");
        // TODO: Actual adf/duplex, etc.
        record.Strings.Add($"uuid={caps.Uuid}");
        record.Strings.Add("cs=color,grayscale,binary");
        record.Strings.Add("is=platen"); // and ,adf
        record.Strings.Add("duplex=F");
        return record;
    }

    public void UnadvertiseDevice(EsclDeviceConfig deviceConfig)
    {
        var uuid = deviceConfig.Capabilities.Uuid;
        if (uuid == null)
        {
            throw new ArgumentException("UUID must be specified");
        }
        if (_serviceProfiles.ContainsKey(uuid))
        {
            ServiceDiscovery.Unadvertise(_serviceProfiles[uuid]);
            _serviceProfiles.Remove(uuid);
        }
        if (_serviceProfiles2.ContainsKey(uuid))
        {
            ServiceDiscovery.Unadvertise(_serviceProfiles2[uuid]);
            _serviceProfiles2.Remove(uuid);
        }
    }

    public void Dispose()
    {
        ServiceDiscovery.Unadvertise();
        ServiceDiscovery.Dispose();
    }
}