using Microsoft.Extensions.Logging;

namespace NAPS2.Escl.Server;

public interface IEsclServer : IDisposable
{
    void AddDevice(EsclDeviceConfig deviceConfig);
    void RemoveDevice(EsclDeviceConfig deviceConfig);
    Task Start();
    void Stop();
    ILogger Logger { get; set; }
}