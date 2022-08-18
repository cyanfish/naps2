using Makaretu.Dns;

namespace NAPS2.Escl.Server;

public class MdnsAdvertiser : IDisposable
{
    private ServiceDiscovery? _sd;

    public void Advertise()
    {
        var service = new ServiceProfile("NAPS2-Canon MP495", "_uscan._tcp", 9898);
        service.AddProperty("txtvers", "1");
        service.AddProperty("Vers", "2.0"); // TODO: verify
        service.AddProperty("rs", "escl");
        service.AddProperty("ty", "NAPS2-Canon MP495");
        service.AddProperty("pdl", "application/pdf,image/jpeg,image/png");
        service.AddProperty("uuid", "0e468f6d-e5dc-4abe-8e9f-ad08d8546b0c");
        service.AddProperty("cs", "color,grayscale,binary");
        service.AddProperty("is", "platen"); // and ,adf
        service.AddProperty("duplex", "F");
        _sd = new ServiceDiscovery();
        _sd.Announce(service);
        _sd.Advertise(service);
    }

    public void Dispose()
    {
        _sd?.Unadvertise();
        _sd?.Dispose();
    }
}