using System.Net;

namespace NAPS2.Escl.Client;

public class EsclService
{
    public required IPAddress Ip { get; init; }
    public required int Port { get; init; }
    public required bool Tls { get; init; }
    public required string RootUrl { get; init; }
}