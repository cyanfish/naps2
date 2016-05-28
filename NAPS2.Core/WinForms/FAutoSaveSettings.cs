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
using System.Linq;
using System.Windows.Forms;
using NAPS2.ImportExport;
using NAPS2.Scan;

namespace NAPS2.WinForms
{
    public partial class FAutoSaveSettings : FormBase
    {
        private readonly DialogHelper dialogHelper;

        private bool result;

        public FAutoSaveSettings(DialogHelper dialogHelper)
        {
            this.dialogHelper = dialogHelper;
            InitializeComponent();
        }

        protected override void OnLoad(object sender, EventArgs e)
        {
            if (ScanProfile.AutoSaveSettings != null)
            {
                txtFilePath.Text = ScanProfile.AutoSaveSettings.FilePath;
                cbClearAfterSave.Checked = ScanProfile.AutoSaveSettings.ClearImagesAfterSaving;
                if (ScanProfile.AutoSaveSettings.Separator == SaveSeparator.FilePerScan)
                {
                    rdFilePerScan.Checked = true;
                }
                else if (ScanProfile.AutoSaveSettings.Separator == SaveSeparator.PatchT)
                {
                    rdSeparateByPatchT.Checked = true;
                }
                else
                {
                    rdFilePerPage.Checked = true;
                }
            }

            new LayoutManager(this)
                .Bind(txtFilePath)
                    .WidthToForm()
                .Bind(btnChooseFolder, btnOK, btnCancel)
                    .RightToForm()
                .Activate();
        }

        public bool Result
        {
            get { return result; }
        }

        public ScanProfile ScanProfile { get; set; }

        private void SaveSettings()
        {
            ScanProfile.AutoSaveSettings = new AutoSaveSettings
            {
                FilePath = txtFilePath.Text,
                ClearImagesAfterSaving = cbClearAfterSave.Checked,
                Separator = rdFilePerScan.Checked ? SaveSeparator.FilePerScan
                          : rdSeparateByPatchT.Checked ? SaveSeparator.PatchT
                          : SaveSeparator.FilePerPage
            };
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFilePath.Text))
            {
                txtFilePath.Focus();
                return;
            }
            result = true;
            SaveSettings();
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnChooseFolder_Click(object sender, EventArgs e)
        {
            string savePath;
            if (dialogHelper.PromptToSavePdfOrImage(null, out savePath))
            {
                txtFilePath.Text = savePath;
            }
        }

        private void linkPlaceholders_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var form = FormFactory.Create<FPlaceholders>();
            form.FileName = txtFilePath.Text;
            if (form.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = form.FileName;
            }
        }

        private void linkPatchCodeInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(FBatchScan.PATCH_CODE_INFO_URL);
        }
    }
}
