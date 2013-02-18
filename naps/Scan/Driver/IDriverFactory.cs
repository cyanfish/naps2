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
namespace NAPS2.Scan.Driver
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
