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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.ImportExport.Images;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    public partial class FImageSettings : FormBase
    {
        private readonly ImageSettingsContainer imageSettingsContainer;
        private readonly IUserConfigManager userConfigManager;
        private readonly DialogHelper dialogHelper;

        public FImageSettings(ImageSettingsContainer imageSettingsContainer, IUserConfigManager userConfigManager, DialogHelper dialogHelper)
        {
            this.imageSettingsContainer = imageSettingsContainer;
            this.userConfigManager = userConfigManager;
            this.dialogHelper = dialogHelper;
            InitializeComponent();
        }

        protected override void OnLoad(object sender, EventArgs e)
        {
            new LayoutManager(this)
                .Bind(btnRestoreDefaults, btnOK, btnCancel)
                    .BottomToForm()
                .Bind(txtJpegQuality, btnOK, btnCancel, btnChooseFolder)
                    .RightToForm()
                .Bind(txtDefaultFilePath, tbJpegQuality, lblWarning)
                    .WidthToForm()
                .Activate();

            UpdateValues(imageSettingsContainer.ImageSettings);
            UpdateEnabled();
            cbRememberSettings.Checked = userConfigManager.Config.ImageSettings != null;
        }

        private void UpdateValues(ImageSettings imageSettings)
        {
            txtDefaultFilePath.Text = imageSettings.DefaultFileName;
            cbSkipSavePrompt.Checked = imageSettings.SkipSavePrompt;
            txtJpegQuality.Text = imageSettings.JpegQuality.ToString(CultureInfo.InvariantCulture);
        }

        private void UpdateEnabled()
        {
            cbSkipSavePrompt.Enabled = Path.IsPathRooted(txtDefaultFilePath.Text);
        }

        private void txtDefaultFilePath_TextChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var imageSettings = new ImageSettings
            {
                DefaultFileName = txtDefaultFilePath.Text,
                SkipSavePrompt = cbSkipSavePrompt.Checked,
                JpegQuality = tbJpegQuality.Value
            };

            imageSettingsContainer.ImageSettings = imageSettings;
            userConfigManager.Config.ImageSettings = cbRememberSettings.Checked ? imageSettings : null;
            userConfigManager.Save();

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnRestoreDefaults_Click(object sender, EventArgs e)
        {
            UpdateValues(new ImageSettings());
            cbRememberSettings.Checked = false;
        }

        private void tbJpegQuality_Scroll(object sender, EventArgs e)
        {
            txtJpegQuality.Text = tbJpegQuality.Value.ToString("G");
        }

        private void txtJpegQuality_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(txtJpegQuality.Text, out value))
            {
                if (value >= tbJpegQuality.Minimum && value <= tbJpegQuality.Maximum)
                {
                    tbJpegQuality.Value = value;
                }
            }
        }

        private void linkPlaceholders_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var form = FormFactory.Create<FPlaceholders>();
            form.FileName = txtDefaultFilePath.Text;
            if (form.ShowDialog() == DialogResult.OK)
            {
                txtDefaultFilePath.Text = form.FileName;
            }
        }

        private void btnChooseFolder_Click(object sender, EventArgs e)
        {
            string savePath;
            if (dialogHelper.SaveImage(txtDefaultFilePath.Text, out savePath))
            {
                txtDefaultFilePath.Text = savePath;
            }
        }
    }
}
