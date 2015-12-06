/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2015  Ben Olden-Cooligan

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
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public partial class FProfiles : FormBase
    {
        private const int DEFAULT_PROFILE_ICON_ID = 3;

        private readonly IProfileManager profileManager;
        private readonly AppConfigManager appConfigManager;
        private readonly IconButtonSizer iconButtonSizer;
        private readonly IScanPerformer scanPerformer;
        private readonly ProfileNameTracker profileNameTracker;

        public FProfiles(IProfileManager profileManager, AppConfigManager appConfigManager, IconButtonSizer iconButtonSizer, IScanPerformer scanPerformer, ProfileNameTracker profileNameTracker)
        {
            this.profileManager = profileManager;
            this.appConfigManager = appConfigManager;
            this.iconButtonSizer = iconButtonSizer;
            this.scanPerformer = scanPerformer;
            this.profileNameTracker = profileNameTracker;
            InitializeComponent();
        }

        public Action<IScannedImage> ImageCallback { get; set; }

        private ScanProfile SelectedProfile
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

        protected override void OnLoad(object sender, EventArgs e)
        {
            lvProfiles.LargeImageList = ilProfileIcons.IconsList;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            UpdateProfiles();
            SelectProfile(x => x.IsDefault);

            var lm = new LayoutManager(this)
                .Bind(lvProfiles)
                    .WidthToForm()
                    .HeightToForm()
                .Bind(btnAdd, btnEdit, btnDelete, btnDone)
                    .BottomToForm()
                .Bind(btnDone, btnScan)
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

        private void UpdateProfiles()
        {
            lvProfiles.Items.Clear();
            foreach (var profile in profileManager.Profiles)
            {
                lvProfiles.Items.Add(profile.DisplayName, profile.IsDefault ? DEFAULT_PROFILE_ICON_ID : profile.IconID);
            }
        }

        private void SelectProfile(Func<ScanProfile, bool> pred)
        {
            int i = 0;
            foreach (var profile in profileManager.Profiles)
            {
                if (pred(profile))
                {
                    lvProfiles.Items[i].Selected = true;
                }
                i++;
            }
            if (profileManager.Profiles.Count == 1)
            {
                lvProfiles.Items[0].Selected = true;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var fedit = FormFactory.Create<FEditScanSettings>();
            fedit.ScanProfile = appConfigManager.Config.DefaultProfileSettings ?? new ScanProfile { Version = ScanProfile.CURRENT_VERSION };
            fedit.ShowDialog();
            if (fedit.Result)
            {
                profileManager.Profiles.Add(fedit.ScanProfile);
                UpdateProfiles();
                SelectProfile(x => x == fedit.ScanProfile);
                profileManager.Save();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lvProfiles.SelectedItems.Count > 0)
            {
                int profileIndex = lvProfiles.SelectedItems[0].Index;
                var fedit = FormFactory.Create<FEditScanSettings>();
                fedit.ScanProfile = profileManager.Profiles[profileIndex];
                fedit.ShowDialog();
                if (fedit.Result)
                {
                    profileManager.Profiles[profileIndex] = fedit.ScanProfile;
                    profileManager.Save();
                    UpdateProfiles();
                    SelectProfile(x => x == fedit.ScanProfile);
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
                    foreach (var profile in profileManager.Profiles.ElementsAt(lvProfiles.SelectedIndices.OfType<int>()))
                    {
                        profileNameTracker.DeletingProfile(profile.DisplayName);
                    }
                    profileManager.Profiles.RemoveAll(lvProfiles.SelectedIndices.OfType<int>());
                    profileManager.Save();
                    UpdateProfiles();
                    lvProfiles_SelectedIndexChanged(null, null);
                }
            }
        }

        private void lvProfiles_ItemActivate(object sender, EventArgs e)
        {
            if (SelectedProfile != null)
            {
                PerformScan();
            }
        }

        private void lvProfiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete && btnDelete.Enabled)
            {
                btnDelete_Click(null, null);
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
                editSettingsForm.ScanProfile = new ScanProfile
                {
                    Version = ScanProfile.CURRENT_VERSION
                };
                editSettingsForm.ShowDialog();
                if (!editSettingsForm.Result)
                {
                    return;
                }
                profileManager.Profiles.Add(editSettingsForm.ScanProfile);
                profileManager.Save();
                UpdateProfiles();
                lvProfiles.SelectedIndices.Add(0);
            }
            if (SelectedProfile == null)
            {
                MessageBox.Show(MiscResources.SelectProfileBeforeScan, MiscResources.ChooseProfile, MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            profileManager.Save();
            scanPerformer.PerformScan(SelectedProfile, new ScanParams(), this, ImageCallback);
            Activate();
        }

        private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SelectedProfile == null)
            {
                e.Cancel = true;
            }
            else
            {
                ctxSetDefault.Enabled = !SelectedProfile.IsDefault;
            }
        }

        private void ctxScan_Click(object sender, EventArgs e)
        {
            PerformScan();
        }

        private void ctxEdit_Click(object sender, EventArgs e)
        {
            btnEdit_Click(null, null);
        }

        private void ctxDelete_Click(object sender, EventArgs e)
        {
            btnDelete_Click(null, null);
        }

        private void ctxSetDefault_Click(object sender, EventArgs e)
        {
            if (SelectedProfile != null)
            {
                profileManager.DefaultProfile = SelectedProfile;
                profileManager.Save();

                UpdateProfiles();
                SelectProfile(x => x.IsDefault);
            }
        }
    }
}
