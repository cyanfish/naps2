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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NAPS2
{
    public partial class FChooseProfile : Form
    {
        private List<CScanSettings> settings;

        private CScanSettings profile;

        public CScanSettings Profile
        {
            get { return profile; }
        }

        public FChooseProfile()
        {
            InitializeComponent();
        }

        private void FChooseProfile_Load(object sender, EventArgs e)
        {
            lvProfiles.LargeImageList = ilProfileIcons.IconsList;
            settings = CSettings.LoadProfiles();
            foreach (CScanSettings profile in settings)
            {
                lvProfiles.Items.Add(profile.DisplayName, profile.IconID);
            }
        }

        private void lvProfiles_ItemActivate(object sender, EventArgs e)
        {
            profile = settings[lvProfiles.SelectedItems[0].Index];
            this.Close();
        }
    }
}
