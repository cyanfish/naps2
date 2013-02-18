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

namespace NAPS2.Scan.Driver.Wia
{
    public class WiaScanDriver : IScanDriver
    {
        public const string DRIVER_NAME = "wia";

        public string DriverName
        {
            get { return DRIVER_NAME; }
        }

        public ScanSettings ScanSettings { get; set; }

        public System.Windows.Forms.IWin32Window DialogParent { get; set; }

        public ScanDevice PromptForDevice()
        {
            if (DialogParent == null)
            {
                throw new InvalidOperationException("IScanDriver.DialogParent must be specified before calling PromptForDevice().");
            }
            return WiaApi.SelectDeviceUI();
        }

        public List<IScannedImage> Scan()
        {
            if (ScanSettings == null)
            {
                throw new InvalidOperationException("IScanDriver.ScanSettings must be specified before calling Scan().");
            }
            if (DialogParent == null)
            {
                throw new InvalidOperationException("IScanDriver.DialogParent must be specified before calling Scan().");
            }
            var result = new List<IScannedImage>();
            var api = new WiaApi(ScanSettings);
            // TODO: Only scan once with ScanSource.GLASS
            // TODO: How to handle that if UseNativeUI is specified?
            // TODO: Send progress event (or something) to update thumbnail/scan UI
            while (true)
            {
                var image = api.GetImage();
                if (image == null)
                {
                    break;
                }
                result.Add(image);
            }
            return result;
        }
    }
}
