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
using System.Drawing;
using System.Linq;

namespace NAPS2.WinForms
{
    public partial class FChooseIcon : FormBase
    {
        private int iconID;

        public FChooseIcon()
        {
            InitializeComponent();
            iconID = -1;
        }

        public int IconID
        {
            get { return iconID; }
            set { iconID = value; }
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            iconID = iconList.SelectedItems[0].Index;
            Close();
        }

        private void FChooseIcon_Load(object sender, EventArgs e)
        {
            iconList.LargeImageList = ilProfileIcons.IconsList;
            int i = 0;
            foreach (Image icon in ilProfileIcons.IconsList.Images)
            {
                iconList.Items.Add("", i);
                i++;
            }
        }
    }
}
