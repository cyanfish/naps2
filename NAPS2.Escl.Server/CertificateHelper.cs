using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
#if !NET
using System.Collections.ObjectModel;
using System.Reflection;
#endif

namespace NAPS2.Escl.Server;

internal static class CertificateHelper
{
    private const string SubjectName = "CN=NAPS2-ESCL-Self-Signed";

#if !NET
    // CertificateRequest isn't available in net462/netstandard2.0, but it may exist at runtime (e.g. net472+)
    private static readonly Type? CertificateRequestType;
    private static readonly ConstructorInfo? CertificateRequestConstructor;
    private static readonly PropertyInfo? CertificateExtensionsProperty;
    private static readonly MethodInfo? CreateSelfSignedMethod;

    static CertificateHelper()
    {
        CertificateRequestType = typeof(RSACertificateExtensions).Assembly.GetType(
            "System.Security.Cryptography.X509Certificates.CertificateRequest");
        CertificateRequestConstructor = CertificateRequestType?.GetConstructor(
            new[] { typeof(string), typeof(RSA), typeof(HashAlgorithmName), typeof(RSASignaturePadding) });
        CertificateExtensionsProperty = CertificateRequestType?.GetProperty("CertificateExtensions");
        CreateSelfSignedMethod = CertificateRequestType?.GetMethod("CreateSelfSigned");
    }
#endif

    // See https://stackoverflow.com/a/65258808/2112909
    public static X509Certificate2? GenerateSelfSignedCertificate(ILogger logger)
    {
        try
        {
            var now = DateTimeOffset.UtcNow;
            var notBefore = now.AddDays(-1);
            var notAfter = now.AddYears(10);
            var keyUsage = new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment, true);
#if NET
            var request = new CertificateRequest(SubjectName, RSA.Create(), HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            request.CertificateExtensions.Add(keyUsage);
            using var cert = request.CreateSelfSigned(notBefore, notAfter);
#else
            if (CertificateRequestType == null)
            {
                logger.LogDebug("CertificateRequest type not available");
                return null;
            }

            var request = CertificateRequestConstructor!.Invoke(new object[]
                { SubjectName, RSA.Create(), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1 });

            var extensions = (Collection<X509Extension>) CertificateExtensionsProperty!.GetValue(request)!;
            extensions.Add(keyUsage);

            using var cert = (X509Certificate2) CreateSelfSignedMethod!.Invoke(request,
                new object[] { notBefore, notAfter })!;
#endif
#pragma warning disable SYSLIB0057 // X509CertificateLoader doesn't work for this
            var pfxCert = new X509Certificate2(cert.Export(X509ContentType.Pfx));
#pragma warning restore SYSLIB0057

            return pfxCert;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating self-signed certificate");
            return null;
        }
    }
}
