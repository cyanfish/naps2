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

namespace NAPS2.WinForms
{
    public partial class FPdfSettings : FormBase
    {
        public FPdfSettings()
        {
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

            UpdateEnabled();
        }

        private void UpdateEnabled()
        {
            bool encrypt = cbEncryptPdf.Checked;
            txtUserPassword.Enabled = txtOwnerPassword.Enabled =
                lblUserPassword.Enabled = lblOwnerPassword.Enabled = encrypt;
            cbAllowAnnotations.Enabled =
                cbAllowContentExtraction.Enabled = cbAllowContentExtractionAccessibility.Enabled =
                    cbAllowDocumentAssembly.Enabled = cbAllowDocumentModification.Enabled = cbAllowFormFilling.Enabled =
                        cbAllowFullQualityPrinting.Enabled = cbAllowPrinting.Enabled = encrypt;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }

        private void btnRestoreDefaults_Click(object sender, EventArgs e)
        {

        }

        private void cbEncryptPdf_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }
    }
}
