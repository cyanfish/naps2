using System;
using NUnit.Framework;

using NAPS2.Scan.Driver;
using NAPS2.Scan.Driver.Wia;
using NAPS2.Scan.Driver.Twain;

namespace NAPS2.Tests.Unit
{
    [TestFixture(Category = "Unit,Fast,Driver")]
    public class DefaultScanDriverFactoryTests
    {
        private DefaultScanDriverFactory factory;

        [SetUp]
        public void SetUp()
        {
            factory = new DefaultScanDriverFactory();
        }

        [TearDown]
        public void TearDown()
        {
            factory = null;
        }

        [Test]
        public void HasDriver_Wia_ReturnsTrue()
        {
            var result = factory.HasDriver(WiaScanDriver.DRIVER_NAME);
            Assert.IsTrue(result);
        }

        [Test]
        public void HasDriver_Twain_ReturnsTrue()
        {
            var result = factory.HasDriver(TwainScanDriver.DRIVER_NAME);
            Assert.IsTrue(result);
        }

        [Test]
        public void CreateDriver_Wia_ReturnsWia()
        {
            var driver = factory.CreateDriver(WiaScanDriver.DRIVER_NAME);
            Assert.AreEqual(driver.GetType(), typeof(WiaScanDriver));
        }

        [Test]
        public void CreateDriver_Twain_ReturnsTwain()
        {
            var driver = factory.CreateDriver(TwainScanDriver.DRIVER_NAME);
            Assert.AreEqual(driver.GetType(), typeof(TwainScanDriver));
        }

        [Test]
        public void CreateDriver_Default_ReturnsWia()
        {
            var driver = factory.CreateDriver(null);
            Assert.AreEqual(driver.GetType(), typeof(WiaScanDriver));
        }
    }
}
