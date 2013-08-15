using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using NAPS2.Config;
using NLog;

namespace NAPS2
{
    public class CultureInitializer
    {
        private readonly UserConfigManager userConfigManager;
        private readonly AppConfigManager appConfigManager;
        private readonly Logger logger;

        public CultureInitializer(UserConfigManager userConfigManager, AppConfigManager appConfigManager, Logger logger)
        {
            this.userConfigManager = userConfigManager;
            this.appConfigManager = appConfigManager;
            this.logger = logger;
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
                    logger.ErrorException("Invalid culture.", e);
                }
            }
        }
    }
}