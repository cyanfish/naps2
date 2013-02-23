using System;
using NUnit.Framework;

using NAPS2.Scan.Driver;
using NAPS2.Scan.Driver.Wia;
using NAPS2.Scan.Driver.Twain;

namespace NAPS2.Tests.Unit
{
    [TestFixture(Category = "Unit,Fast,Driver")]
    public class DriverFactoryTests
    {
        private DriverFactory<IScanDriver> factory;

        [SetUp]
        public void SetUp()
        {
            factory = new DriverFactory<IScanDriver>();
        }

        [TearDown]
        public void TearDown()
        {
            factory = null;
        }

        [Test]
        public void HasDriver_NoRegistration_ReturnsFalse()
        {
            var result = factory.HasDriver(WiaScanDriver.DRIVER_NAME);
            Assert.IsFalse(result);
        }

        [Test]
        public void HasDriver_Registered_ReturnsTrue()
        {
            factory.RegisterDriver(WiaScanDriver.DRIVER_NAME, typeof(WiaScanDriver));
            var result = factory.HasDriver(WiaScanDriver.DRIVER_NAME);
            Assert.IsTrue(result);
        }

        [Test]
        public void HasDriver_Unregistered_ReturnsFalse()
        {
            factory.RegisterDriver(WiaScanDriver.DRIVER_NAME, typeof(WiaScanDriver));
            factory.UnregisterDriver(WiaScanDriver.DRIVER_NAME);
            var result = factory.HasDriver(WiaScanDriver.DRIVER_NAME);
            Assert.IsFalse(result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HasDriver_Null_Throws()
        {
            factory.HasDriver(null);
        }

        [Test]
        public void HasDriver_Garbage_ReturnsFalse()
        {
            factory.RegisterDriver(WiaScanDriver.DRIVER_NAME, typeof(WiaScanDriver));
            var result = factory.HasDriver("garbage");
            Assert.IsFalse(result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterDriver_NullDriverName_Throws()
        {
            factory.RegisterDriver(null, typeof(WiaScanDriver));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterDriver_NullDriverType_Throws()
        {
            factory.RegisterDriver(WiaScanDriver.DRIVER_NAME, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterDriver_WrongDriverType_Throws()
        {
            factory.RegisterDriver(WiaScanDriver.DRIVER_NAME, typeof(object));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterDriver_RegisteredTwice_Throws()
        {
            factory.RegisterDriver(WiaScanDriver.DRIVER_NAME, typeof(WiaScanDriver));
            factory.RegisterDriver(WiaScanDriver.DRIVER_NAME, typeof(WiaScanDriver));
        }

        [Test]
        public void RegisterDriver_Register_NoThrow()
        {
            factory.RegisterDriver(WiaScanDriver.DRIVER_NAME, typeof(WiaScanDriver));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateDriver_Unregistered_Throws()
        {
            factory.RegisterDriver(WiaScanDriver.DRIVER_NAME, typeof(WiaScanDriver));
            factory.UnregisterDriver(WiaScanDriver.DRIVER_NAME);
            factory.CreateDriver(WiaScanDriver.DRIVER_NAME);
        }

        [Test]
        public void CreateDriver_Registered_ReturnsDriver()
        {
            factory.RegisterDriver(WiaScanDriver.DRIVER_NAME, typeof(WiaScanDriver));
            var result = factory.CreateDriver(WiaScanDriver.DRIVER_NAME);
            Assert.AreEqual(result.GetType(), typeof(WiaScanDriver));
        }

        [Test]
        public void CreateDriver_ReRegistered_ReturnsDriver()
        {
            factory.RegisterDriver(WiaScanDriver.DRIVER_NAME, typeof(WiaScanDriver));
            factory.UnregisterDriver(WiaScanDriver.DRIVER_NAME);
            factory.RegisterDriver(WiaScanDriver.DRIVER_NAME, typeof(WiaScanDriver));
            var result = factory.CreateDriver(WiaScanDriver.DRIVER_NAME);
            Assert.AreEqual(result.GetType(), typeof(WiaScanDriver));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateDriver_NullWithUnregisteredDefault_Throws()
        {
            factory.DefaultDriverName = WiaScanDriver.DRIVER_NAME;
            factory.CreateDriver(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateDriver_MissingWithUnregisteredDefault_Throws()
        {
            factory.DefaultDriverName = WiaScanDriver.DRIVER_NAME;
            factory.CreateDriver(TwainScanDriver.DRIVER_NAME);
        }

        [Test]
        public void CreateDriver_NullWithRegisteredDefault_ReturnsDefault()
        {
            factory.RegisterDriver(WiaScanDriver.DRIVER_NAME, typeof(WiaScanDriver));
            factory.DefaultDriverName = WiaScanDriver.DRIVER_NAME;
            var result = factory.CreateDriver(null);
            Assert.AreEqual(result.GetType(), typeof(WiaScanDriver));
        }

        [Test]
        public void CreateDriver_MissingWithRegisteredDefault_ReturnsDefault()
        {
            factory.RegisterDriver(WiaScanDriver.DRIVER_NAME, typeof(WiaScanDriver));
            factory.DefaultDriverName = WiaScanDriver.DRIVER_NAME;
            var result = factory.CreateDriver(TwainScanDriver.DRIVER_NAME);
            Assert.AreEqual(result.GetType(), typeof(WiaScanDriver));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UnregisterDriver_NullDriverName_Throws()
        {
            factory.UnregisterDriver(null);
        }

    }
}
