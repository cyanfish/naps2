namespace NAPS2.Escl;

[Flags]
public enum EsclSecurityPolicy
{
    /// <summary>
    /// Allow both HTTP and HTTPS connections.
    /// </summary>
    None = 0,

    ServerRequireHttps = 1,
    ClientRequireHttps = 2,
    ClientRequireHttpOrTrustedCertificate = 4,

    /// <summary>
    /// Only allow HTTPS connections, but clients will accept self-signed certificates.
    /// </summary>
    RequireHttps = ServerRequireHttps | ClientRequireHttps,

    /// <summary>
    /// Only allow HTTPS connections, and clients will only accept trusted certificates.
    /// </summary>
    RequireTrustedCertificate = RequireHttps | ClientRequireHttpOrTrustedCertificate
}