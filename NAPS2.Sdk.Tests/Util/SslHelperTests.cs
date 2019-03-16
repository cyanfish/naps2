using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using NAPS2.Util;
using Org.BouncyCastle.OpenSsl;
using Xunit;

namespace NAPS2.Sdk.Tests.Util
{
    public class SslHelperTests
    {
        [Fact]
        public void GenerateRootCertificate()
        {
            var (cert, privateKey) = SslHelper.GenerateRootCertificate();

            var reader = new PemReader(new StringReader(cert));
            var obj = reader.ReadPemObject();
            Assert.Equal("CERTIFICATE", obj.Type);
            var certObj = new X509Certificate(obj.Content);
            Assert.Equal("CN=naps2", certObj.Issuer);
            Assert.Equal("CN=naps2", certObj.Subject);
        }
    }
}
