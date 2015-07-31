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
using NAPS2.Config;
using NAPS2.ImportExport.Pdf;

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
                .Bind(btnOK, btnCancel)
                    .RightToForm()
                .Bind(groupMetadata, groupProtection)
                    .WidthToForm()
                .Bind(txtTitle, txtAuthor, txtSubject, txtKeywords, txtOwnerPassword, txtUserPassword)
                    .WidthToForm()
                .Activate();

            UpdateValues(pdfSettingsContainer.PdfSettings);
            UpdateEnabled();
            cbRememberSettings.Checked = userConfigManager.Config.PdfSettings != null;
        }

        private void UpdateValues(PdfSettings pdfSettings)
        {
            txtTitle.Text = pdfSettings.Metadata.Title;
            txtAuthor.Text = pdfSettings.Metadata.Author;
            txtSubject.Text = pdfSettings.Metadata.Subject;
            txtKeywords.Text = pdfSettings.Metadata.Keywords;
            cbEncryptPdf.Checked = pdfSettings.Encryption.EncryptPdf;
            txtOwnerPassword.Text = pdfSettings.Encryption.OwnerPassword;
            txtUserPassword.Text = pdfSettings.Encryption.UserPassword;
            cbAllowContentExtractionAccessibility.Checked = pdfSettings.Encryption.PermitAccessibilityExtractContent;
            cbAllowAnnotations.Checked = pdfSettings.Encryption.PermitAnnotations;
            cbAllowDocumentAssembly.Checked = pdfSettings.Encryption.PermitAssembleDocument;
            cbAllowContentExtraction.Checked = pdfSettings.Encryption.PermitExtractContent;
            cbAllowFormFilling.Checked = pdfSettings.Encryption.PermitFormsFill;
            cbAllowFullQualityPrinting.Checked = pdfSettings.Encryption.PermitFullQualityPrint;
            cbAllowDocumentModification.Checked = pdfSettings.Encryption.PermitModifyDocument;
            cbAllowPrinting.Checked = pdfSettings.Encryption.PermitPrint;
        }

        private void UpdateEnabled()
        {
            bool encrypt = cbEncryptPdf.Checked;
            txtUserPassword.Enabled = txtOwnerPassword.Enabled = cbShowOwnerPassword.Enabled = cbShowUserPassword.Enabled =
                lblUserPassword.Enabled = lblOwnerPassword.Enabled = encrypt;
            cbAllowAnnotations.Enabled =
                cbAllowContentExtraction.Enabled = cbAllowContentExtractionAccessibility.Enabled =
                    cbAllowDocumentAssembly.Enabled = cbAllowDocumentModification.Enabled = cbAllowFormFilling.Enabled =
                        cbAllowFullQualityPrinting.Enabled = cbAllowPrinting.Enabled = encrypt;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var pdfSettings = new PdfSettings
            {
                Metadata =
                {
                    Title = txtTitle.Text,
                    Author = txtAuthor.Text,
                    Subject = txtSubject.Text,
                    Keywords = txtKeywords.Text
                },
                Encryption =
                {
                    EncryptPdf = cbEncryptPdf.Checked,
                    OwnerPassword = txtOwnerPassword.Text,
                    UserPassword = txtUserPassword.Text,
                    PermitAccessibilityExtractContent = cbAllowContentExtractionAccessibility.Checked,
                    PermitAnnotations = cbAllowAnnotations.Checked,
                    PermitAssembleDocument = cbAllowDocumentAssembly.Checked,
                    PermitExtractContent = cbAllowContentExtraction.Checked,
                    PermitFormsFill = cbAllowFormFilling.Checked,
                    PermitFullQualityPrint = cbAllowFullQualityPrinting.Checked,
                    PermitModifyDocument = cbAllowDocumentModification.Checked,
                    PermitPrint = cbAllowPrinting.Checked
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
    }
}
