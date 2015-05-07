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
using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.Scan;

namespace NAPS2.WinForms
{
    public partial class FChooseProfile : FormBase
    {
        private readonly IProfileManager profileManager;
        private readonly IScanPerformer scanPerformer;
        private readonly IconButtonSizer iconButtonSizer;

        public FChooseProfile(IProfileManager profileManager, IScanPerformer scanPerformer, IconButtonSizer iconButtonSizer)
        {
            this.profileManager = profileManager;
            this.scanPerformer = scanPerformer;
            this.iconButtonSizer = iconButtonSizer;
            InitializeComponent();
        }

        public IScanReceiver ScanReceiver { get; set; }

        protected override void OnLoad(object sender, EventArgs e)
        {
            lvProfiles.LargeImageList = ilProfileIcons.IconsList;
            UpdateProfiles();

            iconButtonSizer.WidthOffset = 20;
            iconButtonSizer.PaddingRight = 4;
            iconButtonSizer.ResizeButtons(btnProfiles);

            new LayoutManager(this)
                .Bind(lvProfiles)
                    .WidthToForm()
                    .HeightToForm()
                .Bind(btnDone, btnProfiles)
                    .BottomToForm()
                .Bind(btnDone, btnScan)
                    .RightToForm()
                .Activate();
        }

        private ExtendedScanSettings SelectedProfile
        {
            get
            {
                if (lvProfiles.SelectedIndices.Count == 1)
                {
                    return profileManager.Profiles[lvProfiles.SelectedIndices[0]];
                }
                return null;
            }
        }

        private void UpdateProfiles()
        {
            lvProfiles.Items.Clear();
            foreach (var profile in profileManager.Profiles)
            {
                lvProfiles.Items.Add(profile.DisplayName, profile.IconID);
                if (profile.IsDefault)
                {
                    lvProfiles.Items[lvProfiles.Items.Count - 1].Selected = true;
                }
            }
            if (profileManager.Profiles.Count == 1)
            {
                lvProfiles.Items[0].Selected = true;
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
            Close();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            PerformScan();
        }

        private void PerformScan()
        {
            if (profileManager.Profiles.Count == 0)
            {
                var editSettingsForm = FormFactory.Create<FEditScanSettings>();
                editSettingsForm.ScanSettings = new ExtendedScanSettings { Version = ExtendedScanSettings.CURRENT_VERSION };
                editSettingsForm.ShowDialog();
                if (!editSettingsForm.Result)
                {
                    return;
                }
                profileManager.Profiles.Add(editSettingsForm.ScanSettings);
                profileManager.Save();
                UpdateProfiles();
                lvProfiles.SelectedIndices.Add(0);
            }
            if (SelectedProfile == null)
            {
                MessageBox.Show(MiscResources.SelectProfileBeforeScan, MiscResources.ChooseProfile, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            profileManager.DefaultProfile = SelectedProfile;
            profileManager.Save();
            scanPerformer.PerformScan(SelectedProfile, this, ScanReceiver);
        }

        private void btnProfiles_Click(object sender, EventArgs e)
        {
            FormFactory.Create<FManageProfiles>().ShowDialog();
            UpdateProfiles();
        }
    }
}
