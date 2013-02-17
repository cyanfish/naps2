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

namespace NAPS.twain
{
    public class CTWAINAPI
    {
        Twain tw;
        Form parent;
        CScanSettings settings;

        public static string SelectDeviceUI()
        {
            Twain tw = new Twain();
            if (!tw.Init(Application.OpenForms[0].Handle))
            {
                throw new Exceptions.ENoScannerFound();
            }
            tw.Select();
            return tw.GetCurrentName();
        }

        public CTWAINAPI(CScanSettings settings)
        {
            this.settings = settings;
        }

        public CTWAINAPI(CScanSettings settings, Form pForm)
        {
            parent = pForm;
            tw = new Twain();
            this.settings = settings;
            if (!tw.Init(parent.Handle))
            {
                throw new Exceptions.EScannerNotFound();
            }
            if (!tw.SelectByName(settings.DeviceID))
            {
                throw new Exceptions.EScannerNotFound();
            }
        }

        public List<CScannedImage> Scan()
        {
            FTwainGui fg = new FTwainGui(settings);
            fg.TwainIface = tw;
            fg.ShowDialog(parent);
            return fg.Bitmaps;
        }
    }
}
