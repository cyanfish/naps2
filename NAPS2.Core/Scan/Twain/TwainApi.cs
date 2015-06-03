/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2014  Ben Olden-Cooligan

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
using NAPS2.WinForms;

namespace NAPS2.Scan.Twain
{
    internal class TwainApi
    {
        private readonly IFormFactory formFactory;
        readonly IWin32Window parent;
        readonly ExtendedScanSettings settings;
        readonly Twain tw;

        public TwainApi(ExtendedScanSettings settings, ScanDevice device, IWin32Window pForm, IFormFactory formFactory)
        {
            parent = pForm;
            this.formFactory = formFactory;
            tw = new Twain(settings);
            this.settings = settings;
            if (!tw.InitDSM(parent.Handle))
            {
                throw new DeviceNotFoundException();
            }
            if (!tw.SelectByName(device.ID))
            {
                throw new DeviceNotFoundException();
            }
        }

        public static string SelectDeviceUI()
        {
            var tw = new Twain(null);
            if (!tw.InitDSM(Application.OpenForms[0].Handle))
            {
                throw new NoDevicesFoundException();
            }
            if (!tw.Select())
            {
                return null;
            }
            return tw.GetCurrentName();
        }

        public List<IScannedImage> Scan()
        {
            var fg = formFactory.Create<FTwainGui>();
            fg.Settings = settings;
            fg.TwainIface = tw;
            fg.ShowDialog(parent);
            return fg.Bitmaps;
        }
    }
}
