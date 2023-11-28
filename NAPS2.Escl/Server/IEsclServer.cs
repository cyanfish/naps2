namespace NAPS2.Escl.Server;

public interface IEsclServer : IDisposable
{
    void AddDevice(EsclDeviceConfig deviceConfig);
    void RemoveDevice(EsclDeviceConfig deviceConfig);
    int Port { get; set; }
    void Start();
    void Stop();
}