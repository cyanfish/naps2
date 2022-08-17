using System.Net;

namespace NAPS2.Escl.Client;

public class EsclService
{
    public string Uuid { get; init; }
    public string Name { get; init; }
    public IPAddress Ip { get; init; }
    public int Port { get; init; }
    public bool Tls { get; init; }
    public string RootUrl { get; init; }
}