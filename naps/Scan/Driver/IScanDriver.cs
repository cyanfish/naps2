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
using System.Windows.Forms;

namespace NAPS.Scan.Driver
{
    /// <summary>
    /// An interface for document scanning drivers (e.g. WIA, TWAIN).
    /// </summary>
    public interface IScanDriver
    {
        /// <summary>
        /// Sets the settings used by the driver for scanning.
        /// This must be set before calling Scan.
        /// Some drivers may check for particular implementors and use further information than provided by IScanSettings.
        /// </summary>
        IScanSettings ScanSettings { set; }

        /// <summary>
        /// Sets the parent window used when creating dialogs. This must be set before calling PromptForDevice or Scan.
        /// </summary>
        IWin32Window DialogParent { set; }

        /// <summary>
        /// Prompts the user (via a dialog) to select a scanning device.
        /// </summary>
        /// <returns>The selected device, or null if no device was selected.</returns>
        /// <exception cref="ScanDriverException">Throws a ScanDriverException if an error occurs when reading the available devices.</exception>
        /// <exception cref="InvalidOperationException">Throws an InvalidOperationException if DialogParent has not been set.</exception>
        IScanDevice PromptForDevice();

        /// <summary>
        /// Scans one or more images, interacting with the user as necessary.
        /// </summary>
        /// <returns>A list of scanned images.</returns>
        /// <exception cref="ScanDriverException">Throws a ScanDriverException if an error occurs while scanning.</exception>
        /// /// <exception cref="InvalidOperationException">Throws an InvalidOperationException if ScanSettings or DialogParent has not been set.</exception>
        List<IScannedImage> Scan();
    }
}
