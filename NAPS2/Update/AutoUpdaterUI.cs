using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.WinForms;
using Ninject;

namespace NAPS2.Update
{
    public class AutoUpdaterUI
    {
        private static readonly TimeSpan UpdateCheckInterval = TimeSpan.FromDays(7);

        private readonly UserConfigManager userConfigManager;
        private readonly AppConfigManager appConfigManager;
        private readonly IAutoUpdater autoUpdater;
        private readonly IKernel kernel;

        public AutoUpdaterUI(UserConfigManager userConfigManager, AppConfigManager appConfigManager, IAutoUpdater autoUpdater, IKernel kernel)
        {
            this.userConfigManager = userConfigManager;
            this.appConfigManager = appConfigManager;
            this.autoUpdater = autoUpdater;
            this.kernel = kernel;
        }

        public void OnApplicationStart(IAutoUpdaterClient client)
        {
            if (userConfigManager.Config.LastUpdateCheckDate == null)
            {
                userConfigManager.Config.LastUpdateCheckDate = DateTime.Now;
                userConfigManager.Save();
            }
            if (DateTime.Now - userConfigManager.Config.LastUpdateCheckDate > UpdateCheckInterval)
            {
                PromptToEnableAutomaticUpdates();
                CheckForUpdate(client);
            }
        }

        private void PromptToEnableAutomaticUpdates()
        {
            if (GetAutoUpdateStatus() == AutoUpdateStatus.Unspecified)
            {
                switch (MessageBox.Show(MiscResources.EnableAutoUpdates, MiscResources.AutoUpdates, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        userConfigManager.Config.AutoUpdate = AutoUpdateStatus.Enabled;
                        break;
                    case DialogResult.No:
                        userConfigManager.Config.AutoUpdate = AutoUpdateStatus.Disabled;
                        break;
                }
                userConfigManager.Save();
            }
        }

        private void CheckForUpdate(IAutoUpdaterClient client)
        {
            if (GetAutoUpdateStatus() == AutoUpdateStatus.Enabled)
            {
                autoUpdater.CheckForUpdate().ContinueWith(updateInfo =>
                {
                    if (updateInfo.Result.HasUpdate)
                    {
                        client.UpdateAvailable(updateInfo.Result.VersionInfo);
                    }
                });
            }
        }

        public void PerformUpdate(VersionInfo versionInfo)
        {
            switch (kernel.Get<FUpdate>().ShowDialog())
            {
                case DialogResult.Yes: // Install
                    // TODO: The app process might need to be killed/restarted before/after installing
                    autoUpdater.DownloadAndInstallUpdate(versionInfo);
                    break;
                case DialogResult.OK: // Download
                    var saveDialog = new SaveFileDialog
                    {
                        FileName = versionInfo.FileName
                    };
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        autoUpdater.DownloadUpdate(versionInfo, saveDialog.FileName);
                    }
                    break;
            }
        }

        private AutoUpdateStatus GetAutoUpdateStatus()
        {
            return userConfigManager.Config.AutoUpdate == AutoUpdateStatus.Unspecified
                ? appConfigManager.Config.AutoUpdate
                : userConfigManager.Config.AutoUpdate;
        }
    }
}
