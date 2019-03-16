using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace NAPS2.Util
{
    public static class SslHelper
    {
        private static readonly SecureRandom Random = new SecureRandom();

        private static readonly Lazy<RsaKeyPairGenerator> RsaKeyPairGenerator = new Lazy<RsaKeyPairGenerator>(() =>
        {
            var gen = new RsaKeyPairGenerator();
            gen.Init(new KeyGenerationParameters(Random, 2048));
            return gen;
        });

        public static (string cert, string privateKey) GenerateRootCertificate()
        {
            var keyPair = RsaKeyPairGenerator.Value.GenerateKeyPair();
            return (GetPemString(GenerateCert(keyPair)), GetPemString(keyPair.Private));
        }

        //public static (string cert, string privateKey) GenerateCertificateChain(string rootCert, string rootPrivateKey)
        //{
        //    var keyPair = RsaKeyPairGenerator.Value.GenerateKeyPair();
        //    var req = new Pkcs10CertificationRequest("SHA512WITHRSA", new X509Name("CN=naps2"), keyPair.Public, null, GetKey(rootPrivateKey));
        //    new Org.BouncyCastle.Asn1..X509CertificatePair()
        //    return (null, null);
        //}

        public static X509Certificate GenerateCert(AsymmetricCipherKeyPair keyPair)
        {
            var gen = new X509V3CertificateGenerator();
            gen.SetPublicKey(keyPair.Public);
            gen.SetIssuerDN(new X509Name("CN=naps2"));
            gen.SetSubjectDN(new X509Name("CN=naps2"));
            gen.SetNotBefore(DateTime.Now - TimeSpan.FromDays(2));
            gen.SetNotAfter(DateTime.Now + TimeSpan.FromDays(365));
            gen.SetSerialNumber(new BigInteger(128, Random));
            // V3TbsCertificateGenerator
            return gen.Generate(new Asn1SignatureFactory("SHA512WITHRSA", keyPair.Private, Random));
        }

        private static string GetPemString(object obj)
        {
            var stringWriter = new StringWriter();
            var writer = new Org.BouncyCastle.OpenSsl.PemWriter(stringWriter);
            writer.WriteObject(obj);
            return stringWriter.ToString();
        }

        private static AsymmetricKeyParameter GetKey(string pem)
        {
            var stringReader = new StringReader(pem);
            var reader = new Org.BouncyCastle.OpenSsl.PemReader(stringReader);
            return (AsymmetricKeyParameter)reader.ReadObject();
        }
    }
}
