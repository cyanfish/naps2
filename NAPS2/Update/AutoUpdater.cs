using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Linq;
using System.Text;
using NAPS2.Config;

namespace NAPS2.Update
{
    public class AutoUpdater
    {
        private readonly UserConfigManager _userConfigManager;
        private readonly AppConfigManager _appConfigManager;

        public AutoUpdater(UserConfigManager userConfigManager, AppConfigManager appConfigManager)
        {
            _userConfigManager = userConfigManager;
            _appConfigManager = appConfigManager;
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
