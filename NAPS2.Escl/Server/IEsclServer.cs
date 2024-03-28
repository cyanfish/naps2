using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace NAPS2.Escl.Server;

public interface IEsclServer : IDisposable
{
    void AddDevice(EsclDeviceConfig deviceConfig);
    void RemoveDevice(EsclDeviceConfig deviceConfig);
    Task Start();
    Task Stop();
    public EsclSecurityPolicy SecurityPolicy { get; set; }
    public X509Certificate2? Certificate { get; set; }
    ILogger Logger { get; set; }
}