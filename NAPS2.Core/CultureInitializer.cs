using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using NAPS2.Config;

namespace NAPS2
{
    public class CultureInitializer
    {
        private readonly UserConfigManager userConfigManager;
        private readonly AppConfigManager appConfigManager;

        public CultureInitializer(UserConfigManager userConfigManager, AppConfigManager appConfigManager)
        {
            this.userConfigManager = userConfigManager;
            this.appConfigManager = appConfigManager;
        }

        public void InitCulture()
        {
            var cultureId = userConfigManager.Config.Culture ?? appConfigManager.Config.DefaultCulture;
            if (!String.IsNullOrWhiteSpace(cultureId))
            {
                try
                {
                    var culture = new CultureInfo(cultureId);
                    Thread.CurrentThread.CurrentUICulture = culture;
                    Thread.CurrentThread.CurrentCulture = culture;
                }
                catch (CultureNotFoundException e)
                {
                    Log.ErrorException("Invalid culture.", e);
                }
            }
        }
    }
}