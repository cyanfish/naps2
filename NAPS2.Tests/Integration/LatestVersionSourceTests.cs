using System;
using System.Collections.Generic;
using System.Linq;
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
            const string versionFileUrl = "https://sourceforge.net/p/naps2/code/ci/master/tree/version.xml?format=raw";
            const int editionCount = 4;

            var source = new LatestVersionSource(versionFileUrl, new UrlStreamReader());
            var versionInfos = source.GetLatestVersionInfo().Result;

            // Only assert against vague characteristics of the version infos
            // This way, the test doesn't break when properties are changed in the normal
            // operation of publishing new versions

            // Check that all editions are read
            Assert.AreEqual(editionCount, versionInfos.Count);
            // Check that all editions are unique
            Assert.AreEqual(editionCount, versionInfos.GroupBy(x => x.Edition).Count());
            foreach (var versionInfo in versionInfos)
            {
                // Check that all properties are read (i.e. not null)
                Assert.False(string.IsNullOrWhiteSpace(versionInfo.DownloadUrl));
                Assert.False(string.IsNullOrWhiteSpace(versionInfo.FileName));
                Assert.False(string.IsNullOrWhiteSpace(versionInfo.LatestVersion));
                // Check that string format arguments have been replaced
                Assert.False(versionInfo.DownloadUrl.Contains("{"));
                Assert.False(versionInfo.FileName.Contains("{"));
            }
        }
    }
}
