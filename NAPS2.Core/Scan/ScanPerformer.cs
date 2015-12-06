/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2015  Ben Olden-Cooligan

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
using System.Windows.Forms;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.Scan
{
    public class ScanPerformer : IScanPerformer
    {
        private readonly IScanDriverFactory driverFactory;
        private readonly IErrorOutput errorOutput;

        public ScanPerformer(IScanDriverFactory driverFactory, IErrorOutput errorOutput)
        {
            this.driverFactory = driverFactory;
            this.errorOutput = errorOutput;
        }

        public void PerformScan(ScanProfile scanProfile, ScanParams scanParams, IWin32Window dialogParent, Action<IScannedImage> imageCallback)
        {
            var driver = driverFactory.Create(scanProfile.DriverName);
            driver.DialogParent = dialogParent;
            driver.ScanProfile = scanProfile;
            driver.ScanParams = scanParams;
            try
            {
                if (scanProfile.Device == null)
                {
                    // The profile has no device specified, so prompt the user to choose one
                    var device = driver.PromptForDevice();
                    if (device == null)
                    {
                        // User cancelled
                        return;
                    }
                    driver.ScanDevice = device;
                }
                else
                {
                    // The profile has a device specified, so use it
                    driver.ScanDevice = scanProfile.Device;
                }

                foreach (IScannedImage scannedImage in driver.Scan())
                {
                    imageCallback(scannedImage);
                }
            }
            catch (ScanDriverException e)
            {
                if (e is ScanDriverUnknownException)
                {
                    Log.ErrorException(e.Message, e.InnerException);
                }
                errorOutput.DisplayError(e.Message);
            }
        }
    }
}
