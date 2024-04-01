namespace NAPS2.Escl;

[Flags]
public enum EsclSecurityPolicy
{
    /// <summary>
    /// Allow both HTTP and HTTPS connections.
    /// </summary>
    None = 0,

    ServerDisableHttps = 1,
    ServerRequireHttps = 2,
    ServerRequireTrustedCertificate = 4,
    ClientDisableHttps = 8,
    ClientRequireHttps = 16,
    ClientRequireTrustedCertificate = 32,

    /// <summary>
    /// Only allow HTTPS connections, but clients will accept self-signed certificates.
    /// </summary>
    RequireHttps = ServerRequireHttps | ClientRequireHttps,

    /// <summary>
    /// Only allow HTTPS connections, and clients will only accept trusted certificates.
    /// </summary>
    RequireTrustedCertificate = RequireHttps | ServerRequireTrustedCertificate | ClientRequireTrustedCertificate
}