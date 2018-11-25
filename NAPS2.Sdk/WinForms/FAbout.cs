using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Update;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    partial class FAbout : FormBase
    {
        private readonly IUserConfigManager userConfigManager;
        private readonly UpdateChecker updateChecker;

        private bool hasCheckedForUpdates;
        private UpdateInfo update;

        public FAbout(AppConfigManager appConfigManager, IUserConfigManager userConfigManager, UpdateChecker updateChecker)
        {
            this.userConfigManager = userConfigManager;
            this.updateChecker = updateChecker;

            RestoreFormState = false;
            InitializeComponent();
            labelProductName.Text = AssemblyProduct;
            labelVersion.Text = String.Format(MiscResources.Version, AssemblyVersion);

            // Some of the localization tools I use don't handle line breaks consistently.
            // This compensates by replacing "\n" with actual line breaks. --Ben
            labelCopyright.Text = labelCopyright.Text.Replace("\\n", "\n");
            // Grow the form to fit the copyright text if necessary
            Width = Math.Max(Width, labelCopyright.Right + 25);

            if (appConfigManager.Config.HideDonateButton)
            {
                btnDonate.Visible = false;
            }
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(logoPictureBox)
                    .TopTo(() => Height / 2)
                .Activate();

#if INSTALLER_MSI
            ConditionalControls.Hide(cbCheckForUpdates, 15);
            ConditionalControls.Hide(lblUpdateStatus, 5);
#else
            cbCheckForUpdates.Checked = userConfigManager.Config.CheckForUpdates;
            UpdateControls();
            DoUpdateCheck();
#endif
        }

        private void DoUpdateCheck()
        {
            if (cbCheckForUpdates.Checked)
            {
                updateChecker.CheckForUpdates().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Log.ErrorException("Error checking for updates", task.Exception);
                    }
                    else
                    {
                        userConfigManager.Config.LastUpdateCheckDate = DateTime.Now;
                        userConfigManager.Save();
                    }
                    update = task.Result;
                    hasCheckedForUpdates = true;
                    SafeInvoke(UpdateControls);
                });
            }
        }

        private void UpdateControls()
        {
            const int margin = 5;
            if (cbCheckForUpdates.Checked)
            {
                if (lblUpdateStatus.Visible == false && linkInstall.Visible == false)
                {
                    ConditionalControls.Show(lblUpdateStatus, margin);
                }
                if (hasCheckedForUpdates)
                {
                    if (update == null)
                    {
                        lblUpdateStatus.Text = MiscResources.NoUpdates;
                        lblUpdateStatus.Visible = true;
                        linkInstall.Visible = false;
                    }
                    else
                    {
                        linkInstall.Text = string.Format(MiscResources.Install, update.Name);
                        lblUpdateStatus.Visible = false;
                        linkInstall.Visible = true;
                    }
                }
                else
                {
                    lblUpdateStatus.Text = MiscResources.CheckingForUpdates;
                    lblUpdateStatus.Visible = true;
                    linkInstall.Visible = false;
                }
            }
            else
            {
                ConditionalControls.Hide(lblUpdateStatus, margin);
                ConditionalControls.Hide(linkInstall, margin);
            }
        }

        private void cbCheckForUpdates_CheckedChanged(object sender, EventArgs e)
        {
            userConfigManager.Config.CheckForUpdates = cbCheckForUpdates.Checked;
            userConfigManager.Save();
            UpdateControls();
            DoUpdateCheck();
        }

        private void linkInstall_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (update != null)
            {
                updateChecker.StartUpdate(update);
            }
        }

#region Assembly Attribute Accessors

        private static string GetAssemblyAttributeValue<T>(Func<T, string> selector)
        {
            object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(T), false);
            if (attributes.Length == 0)
            {
                return "";
            }
            return selector((T)attributes[0]);
        }

        public string AssemblyTitle
        {
            get
            {
                string title = GetAssemblyAttributeValue<AssemblyTitleAttribute>(x => x.Title);
                if (string.IsNullOrEmpty(title))
                {
                    title = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);
                }
                return title;
            }
        }

        public string AssemblyVersion => Assembly.GetEntryAssembly().GetName().Version.ToString();

        public string AssemblyDescription => GetAssemblyAttributeValue<AssemblyDescriptionAttribute>(x => x.Description);

        public string AssemblyProduct => GetAssemblyAttributeValue<AssemblyProductAttribute>(x => x.Product);

        public string AssemblyCopyright => GetAssemblyAttributeValue<AssemblyCopyrightAttribute>(x => x.Copyright);

        public string AssemblyCompany => GetAssemblyAttributeValue<AssemblyCompanyAttribute>(x => x.Company);

#endregion

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkLabel1.Text);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkLabel2.Text);
        }

        private void btnDonate_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.naps2.com/donate");
        }
    }
}
