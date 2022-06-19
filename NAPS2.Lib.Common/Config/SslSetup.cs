using NAPS2.Serialization;

namespace NAPS2.Config;

public class SslSetup
{
    public SecureString? WorkerCert { get; set; }
        
    public SecureString? WorkerPrivateKey { get; set; }
}