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

namespace NAPS2.Scan.Twain
{
    internal class TwainApi
    {
        readonly IWin32Window parent;
        readonly ScanSettings settings;
        readonly Twain tw;

        public TwainApi(ScanSettings settings, IWin32Window pForm)
        {
            parent = pForm;
            tw = new Twain();
            this.settings = settings;
            if (!tw.Init(parent.Handle))
            {
                throw new DeviceNotFoundException();
            }
            if (!tw.SelectByName(settings.Device.ID))
            {
                throw new DeviceNotFoundException();
            }
        }

        public static string SelectDeviceUI()
        {
            try
            {
                var tw = new Twain();
                if (!tw.Init(Application.OpenForms[0].Handle))
                {
                    throw new NoDevicesFoundException();
                }
                tw.Select();
                return tw.GetCurrentName();
            }
            catch (ScanDriverException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ScanDriverException(e);
            }
        }

        public List<IScannedImage> Scan()
        {
            var fg = new FTwainGui(settings) { TwainIface = tw };
            fg.ShowDialog(parent);
            return fg.Bitmaps;
        }
    }
}
