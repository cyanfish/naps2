using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Linq;
using System.Text;
using NAPS2.Config;

namespace NAPS2.Update
{
    public class AutoUpdaterUI
    {
        private readonly UserConfigManager userConfigManager;
        private readonly AppConfigManager appConfigManager;

        public AutoUpdaterUI(UserConfigManager userConfigManager, AppConfigManager appConfigManager)
        {
            this.userConfigManager = userConfigManager;
            this.appConfigManager = appConfigManager;
        }

        public void OnApplicationStart()
        {
            PromptToEnableAutomaticUpdates();
            CheckForUpdate();
        }

        private void PromptToEnableAutomaticUpdates()
        {
            throw new NotImplementedException();
        }

        private void CheckForUpdate()
        {
            throw new NotImplementedException();
        }
    }
}
