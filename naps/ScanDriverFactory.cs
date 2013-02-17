using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS
{
    /// <summary>
    /// Manages implementors of IScanDriver. Drivers are specified by their name (e.g. "wia" or "twain").
    /// Drivers can be added or removed by calling RegisterDriver or UnregisterDriver, respectively.
    /// Once a driver has been registered, an instance can be created by calling CreateDriver.
    /// </summary>
    public class ScanDriverFactory
    {
        /// <summary>
        /// Gets the default driver factory. The initially registered drivers are "wia" and "twain".
        /// </summary>
        public static ScanDriverFactory Default { get; private set; }

        static ScanDriverFactory()
        {
            Default = new ScanDriverFactory();
            Default.RegisterDriver(WiaScanDriver.DRIVER_NAME, typeof(WiaScanDriver));
            Default.RegisterDriver(TwainScanDriver.DRIVER_NAME, typeof(TwainScanDriver));
            Default.DefaultDriverName = WiaScanDriver.DRIVER_NAME;
        }

        private Dictionary<string, Type> types = new Dictionary<string, Type>();

        /// <summary>
        /// Registers a driver. Subsequent calls to CreateDriver with the driver's name will return an instance of the specified driver.
        /// </summary>
        /// <param name="driverName">The driver's name (case sensitive).</param>
        /// <param name="type">The driver's class. It must implement IScanDriver.</param>
        public void RegisterDriver(string driverName, Type type)
        {
            if (driverName == null)
            {
                throw new ArgumentNullException("driverName");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!typeof(IScanDriver).IsAssignableFrom(type))
            {
                throw new ArgumentException("The driver class must implement IScanDriver.");
            }
            if (types[driverName] != null)
            {
                throw new ArgumentException("The driver '" + driverName + "' has already been registered. Call UnregisterDriver first.");
            }
            types[driverName] = type;
        }

        /// <summary>
        /// Unregisters a driver.
        /// </summary>
        /// <param name="driverName">The driver's name (case sensitive).</param>
        public void UnregisterDriver(string driverName)
        {
            if (driverName == null)
            {
                throw new ArgumentNullException("driverName");
            }
            types[driverName] = null;
        }

        /// <summary>
        /// Determines if a driver has been registered.
        /// </summary>
        /// <param name="driverName">The driver's name (case sensitive).</param>
        /// <returns>True if the driver has been registered.</returns>
        public bool HasDriver(string driverName)
        {
            return types.ContainsKey(driverName);
        }

        /// <summary>
        /// Gets or sets the name of the default driver (case sensitive) used by CreateDriver when the specified driver is not registered.
        /// </summary>
        public string DefaultDriverName { get; set; }

        /// <summary>
        /// Creates an instance of a driver.
        /// If the driver has not been registered (or the driver name is null), DefaultDriverName will be used instead of the provided name.
        /// </summary>
        /// <param name="driverName">The driver's name (case sensitive).</param>
        /// <returns>The driver instance.</returns>
        public IScanDriver CreateDriver(string driverName)
        {
            Type type = null;
            if (driverName != null)
            {
                type = types[driverName];
            }
            if (type == null)
            {
                if (DefaultDriverName == null)
                {
                    throw new ArgumentException("The driver '" + driverName + "' could not be found and no default driver was specified.");
                }
                type = types[DefaultDriverName];
                if (type == null)
                {
                    throw new ArgumentException("The driver '" + driverName + "' could not be found, and the default driver '" + DefaultDriverName + "' could not be found either.");
                }
            }
            return (IScanDriver)type.GetConstructor(new Type[] { }).Invoke(null, new object[] { });
        }
    }
}
