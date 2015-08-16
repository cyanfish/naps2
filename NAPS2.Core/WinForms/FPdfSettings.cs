/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2015       Luca De Petrillo
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
using NAPS2.ImportExport.Pdf;
using System.Globalization;

namespace NAPS2.WinForms
{
    public partial class FPdfSettings : FormBase
    {
        private readonly PdfSettingsContainer pdfSettingsContainer;
        private readonly IUserConfigManager userConfigManager;

        public FPdfSettings(PdfSettingsContainer pdfSettingsContainer, IUserConfigManager userConfigManager)
        {
            this.pdfSettingsContainer = pdfSettingsContainer;
            this.userConfigManager = userConfigManager;
            InitializeComponent();
        }

        protected override void OnLoad(object sender, EventArgs e)
        {
            new LayoutManager(this)
                .Bind(btnOK, btnCancel, cbShowOwnerPassword, cbShowUserPassword, txtJpegQuality)
                    .RightToForm()
                .Bind(groupMetadata, groupImage, groupProtection)
                    .WidthToForm()
                .Bind(txtDefaultFileName, txtTitle, txtAuthor, txtSubject, txtKeywords, txtOwnerPassword, txtUserPassword, tbJpegQuality, lblImageCompressionInfo)
                    .WidthToForm()
                .Activate();

            UpdateValues(pdfSettingsContainer.PdfSettings);
            UpdateEnabled();
            cbRememberSettings.Checked = userConfigManager.Config.PdfSettings != null;
        }

        private void UpdateValues(PdfSettings pdfSettings)
        {
            txtDefaultFileName.Text = pdfSettings.DefaultFileName;
            txtTitle.Text = pdfSettings.Metadata.Title;
            txtAuthor.Text = pdfSettings.Metadata.Author;
            txtSubject.Text = pdfSettings.Metadata.Subject;
            txtKeywords.Text = pdfSettings.Metadata.Keywords;
            cbEncryptPdf.Checked = pdfSettings.Encryption.EncryptPdf;
            txtOwnerPassword.Text = pdfSettings.Encryption.OwnerPassword;
            txtUserPassword.Text = pdfSettings.Encryption.UserPassword;
            cbAllowContentCopyingForAccessibility.Checked = pdfSettings.Encryption.AllowContentCopyingForAccessibility;
            cbAllowAnnotations.Checked = pdfSettings.Encryption.AllowAnnotations;
            cbAllowDocumentAssembly.Checked = pdfSettings.Encryption.AllowDocumentAssembly;
            cbAllowContentCopying.Checked = pdfSettings.Encryption.AllowContentCopying;
            cbAllowFormFilling.Checked = pdfSettings.Encryption.AllowFormFilling;
            cbAllowFullQualityPrinting.Checked = pdfSettings.Encryption.AllowFullQualityPrinting;
            cbAllowDocumentModification.Checked = pdfSettings.Encryption.AllowDocumentModification;
            cbAllowPrinting.Checked = pdfSettings.Encryption.AllowPrinting;

            cbCompressImagePdf.Checked = pdfSettings.ImageSettings.CompressImages;
            txtJpegQuality.Text = pdfSettings.ImageSettings.JpegQuality.ToString(CultureInfo.InvariantCulture);
        }

        private void UpdateEnabled()
        {
            bool encrypt = cbEncryptPdf.Checked;
            txtUserPassword.Enabled = txtOwnerPassword.Enabled = cbShowOwnerPassword.Enabled = cbShowUserPassword.Enabled =
                lblUserPassword.Enabled = lblOwnerPassword.Enabled = encrypt;
            cbAllowAnnotations.Enabled =
                cbAllowContentCopying.Enabled = cbAllowContentCopyingForAccessibility.Enabled =
                    cbAllowDocumentAssembly.Enabled = cbAllowDocumentModification.Enabled = cbAllowFormFilling.Enabled =
                        cbAllowFullQualityPrinting.Enabled = cbAllowPrinting.Enabled = encrypt;

            bool compressImage = cbCompressImagePdf.Checked;
            txtJpegQuality.Enabled = tbJpegQuality.Enabled = lbCompressImageQuality.Enabled = 
                lblImageCompressionInfo.Enabled =  compressImage;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var pdfSettings = new PdfSettings
            {
                DefaultFileName = txtDefaultFileName.Text,
                Metadata =
                {
                    Title = txtTitle.Text,
                    Author = txtAuthor.Text,
                    Subject = txtSubject.Text,
                    Keywords = txtKeywords.Text
                },
                ImageSettings = 
                {
                    CompressImages = cbCompressImagePdf.Checked,
                    JpegQuality = tbJpegQuality.Value
                },
                Encryption =
                {
                    EncryptPdf = cbEncryptPdf.Checked,
                    OwnerPassword = txtOwnerPassword.Text,
                    UserPassword = txtUserPassword.Text,
                    AllowContentCopyingForAccessibility = cbAllowContentCopyingForAccessibility.Checked,
                    AllowAnnotations = cbAllowAnnotations.Checked,
                    AllowDocumentAssembly = cbAllowDocumentAssembly.Checked,
                    AllowContentCopying = cbAllowContentCopying.Checked,
                    AllowFormFilling = cbAllowFormFilling.Checked,
                    AllowFullQualityPrinting = cbAllowFullQualityPrinting.Checked,
                    AllowDocumentModification = cbAllowDocumentModification.Checked,
                    AllowPrinting = cbAllowPrinting.Checked
                }
            };

            pdfSettingsContainer.PdfSettings = pdfSettings;
            userConfigManager.Config.PdfSettings = cbRememberSettings.Checked ? pdfSettings : null;
            userConfigManager.Save();

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnRestoreDefaults_Click(object sender, EventArgs e)
        {
            UpdateValues(new PdfSettings());
            UpdateEnabled();
            cbRememberSettings.Checked = false;
        }

        private void cbEncryptPdf_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void cbShowOwnerPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtOwnerPassword.UseSystemPasswordChar = !cbShowOwnerPassword.Checked;
        }

        private void cbShowUserPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtUserPassword.UseSystemPasswordChar = !cbShowUserPassword.Checked;
        }

        private void linkPlaceholders_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var form = FormFactory.Create<FPlaceholders>();
            form.FileName = txtDefaultFileName.Text;
            if (form.ShowDialog() == DialogResult.OK)
            {
                txtDefaultFileName.Text = form.FileName;
            }
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

        private void cbCompressImagePdf_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }
    }
}
