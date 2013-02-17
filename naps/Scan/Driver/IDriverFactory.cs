using System;
namespace NAPS
{
    public interface IDriverFactory<T>
    {
        /// <summary>
        /// Creates an instance of a driver.
        /// If the driver has not been registered, a default may be provided.
        /// </summary>
        /// <param name="driverName">The driver's name (case sensitive).</param>
        /// <returns>The driver instance.</returns>
        T CreateDriver(string driverName);

        /// <summary>
        /// Determines if a driver has been registered.
        /// </summary>
        /// <param name="driverName">The driver's name (case sensitive).</param>
        /// <returns>True if the driver has been registered.</returns>
        bool HasDriver(string driverName);
    }
}
