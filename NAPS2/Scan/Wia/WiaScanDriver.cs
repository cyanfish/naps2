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

namespace NAPS2.Scan.Wia
{
    public class WiaScanDriver : IScanDriver
    {
        private readonly IScannedImageFactory scannedImageFactory;

        public WiaScanDriver(IScannedImageFactory scannedImageFactory)
        {
            this.scannedImageFactory = scannedImageFactory;
        }

        public const string DRIVER_NAME = "wia";

        public string DriverName
        {
            get { return DRIVER_NAME; }
        }

        public ExtendedScanSettings ScanSettings { get; set; }

        public ScanDevice ScanDevice { get; set; }

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
            if (ScanDevice == null)
            {
                throw new InvalidOperationException("IScanDriver.ScanDevice must be specified before calling Scan().");
            }
            if (DialogParent == null)
            {
                throw new InvalidOperationException("IScanDriver.DialogParent must be specified before calling Scan().");
            }
            WiaApi api;
            try
            {
                api = new WiaApi(ScanSettings, ScanDevice, scannedImageFactory);
            }
            catch (ScanDriverException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ScanDriverUnknownException(e);
            }
            while (true)
            {
                IScannedImage image;
                try
                {
                    image = api.GetImage();
                }
                catch (ScanDriverException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new ScanDriverUnknownException(e);
                }
                if (image == null)
                {
                    break;
                }
                yield return image;
                if (ScanSettings.PaperSource == ScanSource.Glass)
                {
                    break;
                }
            }
        }
    }
}
