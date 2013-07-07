/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

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
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;
using Ninject;
using NLog;

namespace NAPS2
{
    public class ScanPerformer : IScanPerformer
    {
        private readonly IKernel kernel;
        private readonly Logger logger;

        public ScanPerformer(IKernel kernel, Logger logger)
        {
            this.kernel = kernel;
            this.logger = logger;
        }

        public void PerformScan(ScanSettings scanSettings, IWin32Window dialogParent, IScanReceiver scanReceiver)
        {
            var driver = kernel.Get<IScanDriver>(scanSettings.Device.DriverName);
            driver.DialogParent = dialogParent;
            driver.ScanSettings = scanSettings;

            try
            {
                foreach (IScannedImage scannedImage in driver.Scan())
                {
                    scanReceiver.ReceiveScannedImage(scannedImage);
                    Application.DoEvents();
                }
            }
            catch (ScanDriverException e)
            {
                if (e is ScanDriverUnknownException)
                {
                    logger.ErrorException(e.Message, e.InnerException);
                }
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
