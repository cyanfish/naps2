using Grpc.Core;

namespace NAPS2.Remoting.Network;

public static class CredentialsHelper
{
    public static ChannelCredentials GetClientCreds(string cert, string privateKey) =>
        new SslCredentials(cert, new KeyCertificatePair(cert, privateKey));

    public static ServerCredentials GetServerCreds(string cert, string privateKey) =>
        new SslServerCredentials(
            new[] { new KeyCertificatePair(cert, privateKey) },
            cert,
            SslClientCertificateRequestType.RequestAndRequireAndVerify);
}