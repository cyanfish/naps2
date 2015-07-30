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
using NAPS2.ImportExport.Email;

namespace NAPS2.WinForms
{
    public partial class FEmailSettings : FormBase
    {
        private readonly EmailSettingsContainer emailSettingsContainer;
        private readonly IUserConfigManager userConfigManager;

        public FEmailSettings(EmailSettingsContainer emailSettingsContainer, IUserConfigManager userConfigManager)
        {
            this.emailSettingsContainer = emailSettingsContainer;
            this.userConfigManager = userConfigManager;
            InitializeComponent();
        }

        protected override void OnLoad(object sender, EventArgs e)
        {
            new LayoutManager(this)
                .Bind(btnRestoreDefaults, btnOK, btnCancel)
                    .BottomToForm()
                .Bind(btnOK, btnCancel)
                    .RightToForm()
                .Bind(txtTo, txtCc, txtBcc, txtSubject, txtAttachmentName, txtBody)
                    .WidthToForm()
                .Bind(txtBody)
                    .HeightToForm()
                .Activate();

            UpdateValues(emailSettingsContainer.EmailSettings);
            cbRememberSettings.Checked = userConfigManager.Config.EmailSettings != null;
        }

        private void UpdateValues(EmailSettings emailSettings)
        {
            txtTo.Text = emailSettings.To;
            txtCc.Text = emailSettings.Cc;
            txtBcc.Text = emailSettings.Bcc;
            txtSubject.Text = emailSettings.Subject;
            txtAttachmentName.Text = emailSettings.AttachmentName;
            txtBody.Text = emailSettings.BodyText;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var emailSettings = new EmailSettings
            {
                To = txtTo.Text,
                Cc = txtCc.Text,
                Bcc = txtBcc.Text,
                Subject = txtSubject.Text,
                AttachmentName = txtAttachmentName.Text,
                BodyText = txtBody.Text
            };

            emailSettingsContainer.EmailSettings = emailSettings;
            userConfigManager.Config.EmailSettings = cbRememberSettings.Checked ? emailSettings : null;
            userConfigManager.Save();

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnRestoreDefaults_Click(object sender, EventArgs e)
        {
            UpdateValues(new EmailSettings());
            cbRememberSettings.Checked = false;
        }
    }
}
