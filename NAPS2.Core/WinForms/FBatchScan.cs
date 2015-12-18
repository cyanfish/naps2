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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.Scan;
using NAPS2.Scan.Batch;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.Scan.Twain;
using NAPS2.Util;
using NTwain.Data;

namespace NAPS2.WinForms
{
    public partial class FBatchScan : FormBase
    {
        private const string PATCH_CODE_INFO_URL = "http://www.naps2.com/doc-batch-scan.html#patch-t";

        private readonly IProfileManager profileManager;
        private readonly AppConfigManager appConfigManager;
        private readonly IconButtonSizer iconButtonSizer;
        private readonly IScanPerformer scanPerformer;
        private readonly IUserConfigManager userConfigManager;
        private readonly BatchScanPerformer batchScanPerformer;
        private readonly IErrorOutput errorOutput;

        private bool batchRunning = false;
        private bool cancelBatch = false;

        public FBatchScan(IProfileManager profileManager, AppConfigManager appConfigManager, IconButtonSizer iconButtonSizer, IScanPerformer scanPerformer, IUserConfigManager userConfigManager, BatchScanPerformer batchScanPerformer, IErrorOutput errorOutput)
        {
            this.profileManager = profileManager;
            this.appConfigManager = appConfigManager;
            this.iconButtonSizer = iconButtonSizer;
            this.scanPerformer = scanPerformer;
            this.userConfigManager = userConfigManager;
            this.batchScanPerformer = batchScanPerformer;
            this.errorOutput = errorOutput;
            InitializeComponent();

            RestoreFormState = false;
        }

        public Action<IScannedImage> ImageCallback { get; set; }

        private BatchSettings BatchSettings { get; set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(groupboxScanConfig, groupboxOutput,
                      panelSaveSeparator, panelSaveTo, panelSaveType, panelScanDetails, panelScanType,
                      comboProfile, txtFilePath, lblStatus)
                    .WidthToForm()
                .Bind(btnEditProfile, btnAddProfile, btnStart, btnCancel, btnChooseFolder)
                    .RightToForm()
                .Activate();

            ConditionalControls.LockHeight(this);

            BatchSettings = userConfigManager.Config.LastBatchSettings ?? new BatchSettings();
            UpdateUIFromSettings();
        }

        private void UpdateUIFromSettings()
        {
            UpdateProfiles();

            rdSingleScan.Checked = BatchSettings.ScanType == BatchScanType.Single;
            rdMultipleScansPrompt.Checked = BatchSettings.ScanType == BatchScanType.MultipleWithPrompt;
            rdMultipleScansDelay.Checked = BatchSettings.ScanType == BatchScanType.MultipleWithDelay;

            // TODO: Verify culture (+ vaildation ofc)
            txtNumberOfScans.Text = BatchSettings.ScanCount.ToString(CultureInfo.CurrentCulture);
            txtTimeBetweenScans.Text = BatchSettings.ScanIntervalSeconds.ToString(CultureInfo.CurrentCulture);

            rdLoadIntoNaps2.Checked = BatchSettings.OutputType == BatchOutputType.Load;
            rdSaveToSingleFile.Checked = BatchSettings.OutputType == BatchOutputType.SingleFile;
            rdSaveToMultipleFiles.Checked = BatchSettings.OutputType == BatchOutputType.MultipleFiles;

            rdFilePerScan.Checked = BatchSettings.SaveSeparator == BatchSaveSeparator.FilePerScan;
            rdFilePerPage.Checked = BatchSettings.SaveSeparator == BatchSaveSeparator.FilePerPage;
            rdSeparateByPatchT.Checked = BatchSettings.SaveSeparator == BatchSaveSeparator.PatchT;

            txtFilePath.Text = BatchSettings.SavePath;
        }

        private bool ValidateSettings()
        {
            bool ok = true;

            BatchSettings.ProfileDisplayName = comboProfile.Text;
            if (comboProfile.SelectedIndex == -1)
            {
                ok = false;
                comboProfile.Focus();
            }

            BatchSettings.ScanType = rdMultipleScansPrompt.Checked ? BatchScanType.MultipleWithPrompt
                                   : rdMultipleScansDelay.Checked ? BatchScanType.MultipleWithDelay
                                   : BatchScanType.Single;

            int scanCount;
            if (!int.TryParse(txtNumberOfScans.Text, out scanCount) || scanCount < 0)
            {
                ok = false;
                scanCount = 0;
                txtNumberOfScans.Focus();
            }
            BatchSettings.ScanCount = scanCount;

            double scanInterval;
            if (!double.TryParse(txtTimeBetweenScans.Text, out scanInterval) || scanInterval < 0)
            {
                ok = false;
                scanInterval = 0;
                txtTimeBetweenScans.Focus();
            }
            BatchSettings.ScanIntervalSeconds = scanInterval;

            BatchSettings.OutputType = rdSaveToSingleFile.Checked ? BatchOutputType.SingleFile
                                     : rdSaveToMultipleFiles.Checked ? BatchOutputType.MultipleFiles
                                     : BatchOutputType.Load;

            BatchSettings.SaveSeparator = rdFilePerScan.Checked ? BatchSaveSeparator.FilePerScan
                                        : rdSeparateByPatchT.Checked ? BatchSaveSeparator.PatchT
                                        : BatchSaveSeparator.FilePerPage;

            BatchSettings.SavePath = txtFilePath.Text;
            if (BatchSettings.OutputType != BatchOutputType.Load && string.IsNullOrWhiteSpace(BatchSettings.SavePath))
            {
                ok = false;
                txtFilePath.Focus();
            }

            return ok;
        }

        private void UpdateProfiles()
        {
            comboProfile.Items.Clear();
            comboProfile.Items.AddRange(profileManager.Profiles.Cast<object>().ToArray());
            comboProfile.Text = BatchSettings.ProfileDisplayName ?? profileManager.DefaultProfile.DisplayName;
            ProfileChanged();
        }

        private bool ProfileIsTwain()
        {
            var profile = (ScanProfile) comboProfile.SelectedItem;
            if (profile != null)
            {
                return profile.DriverName == TwainScanDriver.DRIVER_NAME;
            }
            return false;
        }

        private void ProfileChanged()
        {
            rdSeparateByPatchT.Enabled = ProfileIsTwain();
        }

        private void rdSingleScan_CheckedChanged(object sender, EventArgs    e)
        {
            ConditionalControls.UnlockHeight(this);
            ConditionalControls.SetVisible(rdFilePerScan, !rdSingleScan.Checked && rdSaveToMultipleFiles.Checked);
            ConditionalControls.LockHeight(this);
        }

        private void rdMultipleScansDelay_CheckedChanged(object sender, EventArgs e)
        {
            ConditionalControls.UnlockHeight(this);
            ConditionalControls.SetVisible(panelScanDetails, rdMultipleScansDelay.Checked);
            ConditionalControls.LockHeight(this);
        }

        private void rdLoadIntoNaps2_CheckedChanged(object sender, EventArgs e)
        {
            ConditionalControls.UnlockHeight(this);
            ConditionalControls.SetVisible(panelSaveTo, !rdLoadIntoNaps2.Checked);
            ConditionalControls.LockHeight(this);
        }

        private void rdSaveToMultipleFiles_CheckedChanged(object sender, EventArgs e)
        {
            ConditionalControls.UnlockHeight(this);
            ConditionalControls.SetVisible(panelSaveSeparator, rdSaveToMultipleFiles.Checked);
            ConditionalControls.SetVisible(rdFilePerScan, !rdSingleScan.Checked && rdSaveToMultipleFiles.Checked);
            ConditionalControls.LockHeight(this);
        }

        private void btnChooseFolder_Click(object sender, EventArgs e)
        {
            var sd = new SaveFileDialog
            {
                OverwritePrompt = false,
                AddExtension = true,
                Filter = MiscResources.FileTypePdf + "|*.pdf|" +
                         MiscResources.FileTypeBmp + "|*.bmp|" +
                         MiscResources.FileTypeEmf + "|*.emf|" +
                         MiscResources.FileTypeExif + "|*.exif|" +
                         MiscResources.FileTypeGif + "|*.gif|" +
                         MiscResources.FileTypeJpeg + "|*.jpg;*.jpeg|" +
                         MiscResources.FileTypePng + "|*.png|" +
                         MiscResources.FileTypeTiff + "|*.tiff;*.tif",
            };
            if (sd.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = sd.FileName;
            }
        }

        private void linkPatchCodeInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(PATCH_CODE_INFO_URL);
        }

        private void comboProfile_Format(object sender, ListControlConvertEventArgs e)
        {
            e.Value = ((ScanProfile)e.ListItem).DisplayName;
        }

        private void btnEditProfile_Click(object sender, EventArgs e)
        {
            if (comboProfile.SelectedItem != null)
            {
                var fedit = FormFactory.Create<FEditScanSettings>();
                fedit.ScanProfile = (ScanProfile)comboProfile.SelectedItem;
                fedit.ShowDialog();
                if (fedit.Result)
                {
                    profileManager.Profiles[comboProfile.SelectedIndex] = fedit.ScanProfile;
                    profileManager.Save();
                    BatchSettings.ProfileDisplayName = fedit.ScanProfile.DisplayName;
                    UpdateProfiles();
                }
            }
        }

        private void btnAddProfile_Click(object sender, EventArgs e)
        {
            var fedit = FormFactory.Create<FEditScanSettings>();
            fedit.ScanProfile = appConfigManager.Config.DefaultProfileSettings ?? new ScanProfile { Version = ScanProfile.CURRENT_VERSION };
            fedit.ShowDialog();
            if (fedit.Result)
            {
                profileManager.Profiles.Add(fedit.ScanProfile);
                profileManager.Save();
                BatchSettings.ProfileDisplayName = fedit.ScanProfile.DisplayName;
                UpdateProfiles();
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (batchRunning)
            {
                return;
            }
            if (!ValidateSettings())
            {
                return;
            }

            // Update state
            batchRunning = true;
            cancelBatch = false;

            // Update UI
            btnStart.Enabled = false;
            btnCancel.Enabled = true;
            btnCancel.Text = MiscResources.Cancel;
            EnableDisableSettings(false);

            // Start the batch
            Task.Factory.StartNew(DoBatchScan);

            // Save settings for next time (could also do on form close)
            userConfigManager.Config.LastBatchSettings = BatchSettings;
            userConfigManager.Save();
        }

        private void EnableDisableSettings(bool enabled)
        {
            EnableDisable(groupboxScanConfig, enabled);
            EnableDisable(groupboxOutput, enabled);
            rdSeparateByPatchT.Enabled = enabled && ProfileIsTwain();
        }

        private void EnableDisable(Control root, bool enabled)
        {
            foreach (Control control in root.Controls)
            {
                if (control.Controls.Count > 0)
                {
                    EnableDisable(control, enabled);
                }
                else
                {
                    control.Enabled = enabled;
                }
            }
        }

        private void DoBatchScan()
        {
            try
            {
                var profile = profileManager.Profiles.First(x => x.DisplayName == BatchSettings.ProfileDisplayName);
                if (profile.DriverName == TwainScanDriver.DRIVER_NAME || profile.UseNativeUI)
                {
                    // We would prefer not to run this on the UI thread, but will if necessary
                    Invoke(new Action(() => batchScanPerformer.PerformBatchScan(BatchSettings, this, image => Invoke(new Action(() => ImageCallback(image))), ProgressCallback())));
                }
                else
                {
                    batchScanPerformer.PerformBatchScan(BatchSettings, this, image => Invoke(new Action(() => ImageCallback(image))), ProgressCallback());
                }
                Invoke(new Action(() =>
                {
                    lblStatus.Text = cancelBatch
                        ? MiscResources.BatchStatusCancelled
                        : MiscResources.BatchStatusComplete;
                }));
            }
            catch (ScanDriverException ex)
            {
                if (ex is ScanDriverUnknownException)
                {
                    Log.ErrorException("Error in batch scan", ex);
                }
                errorOutput.DisplayError(ex.Message);
            }
            catch (Exception ex)
            {
                Log.ErrorException("Error in batch scan", ex);
                errorOutput.DisplayError(MiscResources.BatchError);
                Invoke(new Action(() =>
                {
                    lblStatus.Text = MiscResources.BatchStatusError;
                }));
            }
            Invoke(new Action(() =>
            {
                batchRunning = false;
                cancelBatch = false;
                btnStart.Enabled = true;
                btnCancel.Enabled = true;
                btnCancel.Text = MiscResources.Close;
                EnableDisableSettings(true);
                Activate();
            }));
        }

        private Func<string, bool> ProgressCallback()
        {
            return status =>
            {
                if (!cancelBatch)
                {
                    Invoke(new Action(() =>
                    {
                        lblStatus.Text = status;
                    }));
                }
                return !cancelBatch;
            };
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (batchRunning)
            {
                cancelBatch = true;
                btnCancel.Enabled = false;
                lblStatus.Text = MiscResources.BatchStatusCancelling;
            }
            else
            {
                Close();
            }
        }

        private void FBatchScan_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cancelBatch)
            {
                // Keep dialog open while cancel is in progress to avoid concurrency issues
                e.Cancel = true;
            }
        }

        private void linkSubstitutions_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var form = FormFactory.Create<FPlaceholders>();
            form.FileName = txtFilePath.Text;
            if (form.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = form.FileName;
            }
        }

        private void comboProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProfileChanged();
        }
    }
}
