/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
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

using NAPS2.Scan;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Ninject;

namespace NAPS2
{
    public partial class FChooseProfile : Form
    {
        private readonly IProfileManager profileManager;
        private readonly IScanPerformer scanPerformer;
        private readonly IScanReceiver scanReceiver;

        private ScanSettings SelectedProfile
        {
            get
            {
                if (lvProfiles.SelectedIndices.Count == 1)
                {
                    return profileManager.Profiles[lvProfiles.SelectedIndices[0]];
                }
                else
                {
                    return null;
                }
            }
        }

        public FChooseProfile(IProfileManager profileManager, IScanPerformer scanPerformer, IScanReceiver scanReceiver)
        {
            this.profileManager = profileManager;
            this.scanPerformer = scanPerformer;
            this.scanReceiver = scanReceiver;
            InitializeComponent();
        }

        private void FChooseProfile_Load(object sender, EventArgs e)
        {
            lvProfiles.LargeImageList = ilProfileIcons.IconsList;
            UpdateProfiles();
        }

        private void UpdateProfiles()
        {
            lvProfiles.Items.Clear();
            foreach (var profile in profileManager.Profiles)
            {
                lvProfiles.Items.Add(profile.DisplayName, profile.IconID);
            }
        }

        private void lvProfiles_ItemActivate(object sender, EventArgs e)
        {
            if (SelectedProfile != null)
            {
                PerformScan();
            }
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            PerformScan();
        }

        private void PerformScan()
        {
            if (profileManager.Profiles.Count == 0)
            {
                var editSettingsForm = KernelManager.Kernel.Get<FEditScanSettings>();
                editSettingsForm.ScanSettings = new ExtendedScanSettings();
                editSettingsForm.ShowDialog();
                if (editSettingsForm.Result)
                {
                    profileManager.Profiles.Add(editSettingsForm.ScanSettings);
                    profileManager.Save();
                    UpdateProfiles();
                    lvProfiles.SelectedIndices.Add(0);
                }
            }
            if (SelectedProfile == null)
            {
                MessageBox.Show("Select a profile before clicking Scan.", "Choose Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            scanPerformer.PerformScan(SelectedProfile, this, scanReceiver);
        }

        private void btnProfiles_Click(object sender, EventArgs e)
        {
            KernelManager.Kernel.Get<FManageProfiles>().ShowDialog();
            UpdateProfiles();
        }
    }
}
