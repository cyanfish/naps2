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
using NAPS2.Scan.Exceptions;
using NLog;

namespace NAPS2.Scan.Wia
{
    public class WiaScanDriver : IScanDriver
    {
        public const string DRIVER_NAME = "wia";

        private readonly Logger logger;

        public WiaScanDriver(Logger logger)
        {
            this.logger = logger;
        }

        public string DriverName
        {
            get { return DRIVER_NAME; }
        }

        public ScanSettings ScanSettings { get; set; }

        public IWin32Window DialogParent { get; set; }

        public ScanDevice PromptForDevice()
        {
            if (DialogParent == null)
            {
                throw new InvalidOperationException("IScanDriver.DialogParent must be specified before calling PromptForDevice().");
            }
            try
            {
                return WiaApi.SelectDeviceUI();
            }
            catch (ScanDriverException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ScanDriverUnknownException(e);
            }
        }

        public IEnumerable<IScannedImage> Scan()
        {
            if (ScanSettings == null)
            {
                throw new InvalidOperationException("IScanDriver.ScanSettings must be specified before calling Scan().");
            }
            if (DialogParent == null)
            {
                throw new InvalidOperationException("IScanDriver.DialogParent must be specified before calling Scan().");
            }
            WiaApi api;
            try
            {
                api = new WiaApi(ScanSettings, logger);
            }
            catch (ScanDriverException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ScanDriverUnknownException(e);
            }
            logger.Trace("Beginning WIA scan");
            while (true)
            {
                ScannedImage image;
                try
                {
                    logger.Trace("Reading image from WIA API");
                    image = api.GetImage();
                }
                catch (ScanDriverException)
                {
                    logger.Trace("ScanDriverException");
                    throw;
                }
                catch (Exception e)
                {
                    logger.Trace("ScanDriverUnknownException");
                    throw new ScanDriverUnknownException(e);
                }
                if (image == null)
                {
                    logger.Trace("Null image from WIA API, ending scan");
                    break;
                }
                logger.Trace("Yielding image");
                yield return image;
                var extSettings = ScanSettings as ExtendedScanSettings;
                if (extSettings != null && extSettings.PaperSource == ScanSource.Glass)
                {
                    logger.Trace("PaperSource is Glass, ending scan after first image");
                    break;
                }
            }
            logger.Trace("Scan ended");
        }
    }
}
