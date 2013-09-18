using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAPS2.Update;
using NUnit.Framework;

namespace NAPS2.Tests.Integration
{
    [TestFixture(Category = "Integration,Slow")]
    public class LatestVersionSourceTests
    {
        [Test]
        public void GetLatestVersion_ReadsVersionFromSourceforge()
        {
            const string versionFileUrl = "https://sourceforge.net/p/naps2/code/ci/master/tree/version.txt?format=raw";
            var source = new LatestVersionSource(versionFileUrl, new UrlTextReader(new UrlStreamReader()));
            var version = source.GetLatestVersion().Result;
            Assert.AreEqual(version, "2.6");
        }
    }
}
