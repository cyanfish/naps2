using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.Logging;

namespace NAPS2.Util
{
    /// <summary>
    /// A helper to set the thread culture based on user and app configuration.
    /// </summary>
    public class CultureInitializer
    {
        private readonly IUserConfigManager userConfigManager;
        private readonly AppConfigManager appConfigManager;

        public CultureInitializer(IUserConfigManager userConfigManager, AppConfigManager appConfigManager)
        {
            this.userConfigManager = userConfigManager;
            this.appConfigManager = appConfigManager;
        }

        public void InitCulture(Thread thread)
        {
            var cultureId = userConfigManager.Config.Culture ?? appConfigManager.Config.DefaultCulture;
            if (!string.IsNullOrWhiteSpace(cultureId))
            {
                try
                {
                    var culture = new CultureInfo(cultureId);
                    thread.CurrentUICulture = culture;
                    thread.CurrentCulture = culture;
                    MiscResources.Culture = culture;
                    SettingsResources.Culture = culture;
                }
                catch (CultureNotFoundException e)
                {
                    Log.ErrorException("Invalid culture.", e);
                }
            }
        }
    }
}