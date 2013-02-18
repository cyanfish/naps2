/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009        Pavel Sorejs
    Copyright (C) 2012, 2013  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Scan.Driver
{
    /// <summary>
    /// Manages implementors of T. Drivers are specified by their name.
    /// Drivers can be added or removed by calling RegisterDriver or UnregisterDriver, respectively.
    /// Once a driver has been registered, an instance can be created by calling CreateDriver.
    /// </summary>
    public class DriverFactory<T> : IDriverFactory<T>
    {
        private Dictionary<string, Type> types = new Dictionary<string, Type>();

        /// <summary>
        /// Registers a driver. Subsequent calls to CreateDriver with the driver's name will return an instance of the specified driver.
        /// </summary>
        /// <param name="driverName">The driver's name (case sensitive).</param>
        /// <param name="driverType">The driver's class. It must implement T.</param>
        public void RegisterDriver(string driverName, Type driverType)
        {
            if (driverName == null)
            {
                throw new ArgumentNullException("driverName");
            }
            if (driverType == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!typeof(T).IsAssignableFrom(driverType))
            {
                throw new ArgumentException("The driver class must implement " + typeof(T).Name + ".");
            }
            if (types.ContainsKey(driverName))
            {
                throw new ArgumentException("The driver '" + driverName + "' has already been registered. Call UnregisterDriver first.");
            }
            types[driverName] = driverType;
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
            types.Remove(driverName);
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
        public T CreateDriver(string driverName)
        {
            Type type = null;
            if (driverName != null && types.ContainsKey(driverName))
            {
                type = types[driverName];
            }
            else
            {
                if (DefaultDriverName == null)
                {
                    throw new ArgumentException("The driver '" + driverName + "' could not be found and no default driver was specified.");
                }
                if (types.ContainsKey(DefaultDriverName))
                {
                    type = types[DefaultDriverName];
                }
                else
                {
                    throw new ArgumentException("The driver '" + driverName + "' could not be found, and the default driver '" + DefaultDriverName + "' could not be found either.");
                }
            }
            return (T)type.GetConstructor(new Type[] { }).Invoke(new object[] { });
        }
    }
}
