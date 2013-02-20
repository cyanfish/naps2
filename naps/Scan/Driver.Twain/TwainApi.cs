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
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using NAPS2.Scan;

namespace NAPS2.Scan.Driver.Twain
{
    internal class TwainApi
    {
        Twain tw;
        IWin32Window parent;
        ScanSettings settings;

        public static string SelectDeviceUI()
        {
            try
            {
                Twain tw = new Twain();
                if (!tw.Init(Application.OpenForms[0].Handle))
                {
                    throw new NoDevicesFoundException();
                }
                tw.Select();
                return tw.GetCurrentName();
            }
            catch (ScanDriverException e)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ScanDriverException(e);
            }
        }

        public TwainApi(ScanSettings settings)
        {
            this.settings = settings;
        }

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

        public List<IScannedImage> Scan()
        {
            FTwainGui fg = new FTwainGui(settings);
            fg.TwainIface = tw;
            fg.ShowDialog(parent);
            return fg.Bitmaps;
        }
    }
}
