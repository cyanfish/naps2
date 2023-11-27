using Makaretu.Dns;

namespace NAPS2.Escl.Server;

public class MdnsAdvertiser : IDisposable
{
    private readonly ServiceDiscovery _sd;
    private readonly Dictionary<string, ServiceProfile> _serviceProfiles = new();

    public MdnsAdvertiser()
    {
        _sd = new ServiceDiscovery();
    }

    public void AdvertiseDevice(EsclDeviceConfig deviceConfig)
    {
        var caps = deviceConfig.Capabilities;
        if (caps.Uuid == null)
        {
            throw new ArgumentException("UUID must be specified");
        }
        var name = caps.MakeAndModel;
        var service = new ServiceProfile(name, "_uscan._tcp", (ushort) deviceConfig.Port);
        var domain = $"naps2-{caps.Uuid}";
        service.HostName = DomainName.Join(domain, service.Domain);
        service.AddProperty("txtvers", "1");
        service.AddProperty("Vers", "2.0"); // TODO: verify
        if (deviceConfig.Capabilities.IconPng != null)
        {
            service.AddProperty("representation", $"http://naps2-{caps.Uuid}.local.:{deviceConfig.Port}/eSCL/icon.png");
        }
        service.AddProperty("rs", "eSCL");
        service.AddProperty("ty", name);
        service.AddProperty("pdl", "application/pdf,image/jpeg,image/png");
        // TODO: Actual adf/duplex, etc.
        service.AddProperty("uuid", caps.Uuid);
        service.AddProperty("cs", "color,grayscale,binary");
        service.AddProperty("is", "platen"); // and ,adf
        service.AddProperty("duplex", "F");
        _sd.Announce(service);
        _sd.Advertise(service);
        _serviceProfiles.Add(caps.Uuid, service);
    }

    public void UnadvertiseDevice(EsclDeviceConfig deviceConfig)
    {
        if (deviceConfig.Capabilities.Uuid == null)
        {
            throw new ArgumentException("UUID must be specified");
        }
        _sd.Unadvertise(_serviceProfiles[deviceConfig.Capabilities.Uuid]);
        _serviceProfiles.Remove(deviceConfig.Capabilities.Uuid);
    }

    public void Dispose()
    {
        _sd.Unadvertise();
        _sd.Dispose();
    }
}