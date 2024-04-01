using System.Collections.ObjectModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace NAPS2.Escl.Server;

internal static class CertificateHelper
{
    private static readonly Type? CertificateRequestType;
    private static readonly ConstructorInfo? CertificateRequestConstructor;
    private static readonly PropertyInfo? CertificateExtensionsProperty;
    private static readonly MethodInfo? CreateSelfSignedMethod;

    static CertificateHelper()
    {
        // TODO: On net472+ we can avoid the reflection
        CertificateRequestType = typeof(RSACertificateExtensions).Assembly.GetType(
            "System.Security.Cryptography.X509Certificates.CertificateRequest");
        CertificateRequestConstructor = CertificateRequestType?.GetConstructor(
            new[] { typeof(string), typeof(RSA), typeof(HashAlgorithmName), typeof(RSASignaturePadding) });
        CertificateExtensionsProperty = CertificateRequestType?.GetProperty("CertificateExtensions");
        CreateSelfSignedMethod = CertificateRequestType?.GetMethod("CreateSelfSigned");
    }

    // See https://stackoverflow.com/a/65258808/2112909
    public static X509Certificate2? GenerateSelfSignedCertificate(ILogger logger)
    {
        try
        {
            if (CertificateRequestType == null)
            {
                logger.LogDebug("CertificateRequest type not available");
                return null;
            }

            var request = CertificateRequestConstructor!.Invoke(new object[]
                { "CN=NAPS2-ESCL-Self-Signed", RSA.Create(), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1 });

            var extensions = (Collection<X509Extension>) CertificateExtensionsProperty!.GetValue(request);
            extensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment, true));

            var now = DateTimeOffset.UtcNow;
            var cert = (X509Certificate2) CreateSelfSignedMethod!.Invoke(request,
                new object[] { now.AddDays(-1), now.AddYears(10) });
            var pfxCert = new X509Certificate2(cert.Export(X509ContentType.Pfx));

            return pfxCert;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating self-signed certificate");
            return null;
        }
    }
}