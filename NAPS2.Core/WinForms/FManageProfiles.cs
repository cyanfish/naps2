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
    public partial class FManageProfiles : FormBase
    {
        private readonly IProfileManager profileManager;
        private readonly AppConfigManager appConfigManager;
        private readonly IconButtonSizer iconButtonSizer;

        public FManageProfiles(IProfileManager profileManager, AppConfigManager appConfigManager, IconButtonSizer iconButtonSizer)
        {
            this.profileManager = profileManager;
            this.appConfigManager = appConfigManager;
            this.iconButtonSizer = iconButtonSizer;
            InitializeComponent();
        }

        protected override void OnLoad(object sender, EventArgs e)
        {
            lvProfiles.LargeImageList = ilProfileIcons.IconsList;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            LoadProfileList();

            var lm = new LayoutManager(this)
                .Bind(lvProfiles)
                    .WidthToForm()
                    .HeightToForm()
                .Bind(btnAdd, btnEdit, btnDelete, btnOK)
                    .BottomToForm()
                .Bind(btnOK)
                    .RightToForm()
                .Bind(btnEdit)
                    .LeftTo(() => btnAdd.Right)
                .Bind(btnDelete)
                    .LeftTo(() => btnEdit.Right)
                .Activate();

            iconButtonSizer.WidthOffset = 20;
            iconButtonSizer.PaddingRight = 4;
            iconButtonSizer.MaxWidth = 100;
            iconButtonSizer.ResizeButtons(btnAdd, btnEdit, btnDelete);

            lm.UpdateLayout();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void LoadProfileList()
        {
            lvProfiles.Items.Clear();
            foreach (var profile in profileManager.Profiles)
            {
                lvProfiles.Items.Add(profile.DisplayName, profile.IconID);
            }

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var fedit = FormFactory.Create<FEditScanSettings>();
            fedit.ScanSettings = appConfigManager.Config.DefaultProfileSettings ?? new ExtendedScanSettings { Version = ExtendedScanSettings.CURRENT_VERSION };
            fedit.ShowDialog();
            if (fedit.Result)
            {
                profileManager.Profiles.Add(fedit.ScanSettings);
                LoadProfileList();
                profileManager.Save();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lvProfiles.SelectedItems.Count > 0)
            {
                int profileIndex = lvProfiles.SelectedItems[0].Index;
                var fedit = FormFactory.Create<FEditScanSettings>();
                fedit.ScanSettings = profileManager.Profiles[profileIndex];
                fedit.ShowDialog();
                if (fedit.Result)
                {
                    profileManager.Profiles[profileIndex] = fedit.ScanSettings;
                    profileManager.Save();
                    LoadProfileList();
                    lvProfiles.SelectedIndices.Add(profileIndex);
                }
                else
                {
                    // Rollback
                    profileManager.Load();
                }
            }
        }

        private void lvProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnEdit.Enabled = lvProfiles.SelectedItems.Count == 1;
            btnDelete.Enabled = lvProfiles.SelectedItems.Count > 0;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lvProfiles.SelectedItems.Count > 0)
            {
                string message = lvProfiles.SelectedIndices.Count == 1
                    ? string.Format(MiscResources.ConfirmDeleteSingleProfile, profileManager.Profiles[lvProfiles.SelectedIndices[0]].DisplayName)
                    : string.Format(MiscResources.ConfirmDeleteMultipleProfiles, lvProfiles.SelectedIndices.Count);
                if (MessageBox.Show(message, MiscResources.Delete, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    profileManager.Profiles.RemoveAll(lvProfiles.SelectedIndices.OfType<int>());
                    profileManager.Save();
                    LoadProfileList();
                    lvProfiles_SelectedIndexChanged(null, null);
                }
            }
        }

        private void lvProfiles_ItemActivate(object sender, EventArgs e)
        {
            btnEdit_Click(null, null);
        }

        private void lvProfiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete && btnDelete.Enabled)
            {
                btnDelete_Click(null, null);
            }
        }

    }
}
