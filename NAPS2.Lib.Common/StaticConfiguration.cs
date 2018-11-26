using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;
using NAPS2.Logging;
using NAPS2.Platform;

namespace NAPS2.DI
{
    public static class StaticConfiguration
    {
        public static void Initialize()
        {
            Log.Logger = new NLogLogger();
            if (PlatformCompat.System.CanUseWin32)
            {
                Log.EventLogger = new WindowsEventLogger();
            }

            UserConfig.Manager = new ConfigManager<UserConfig>("config.xml", Paths.AppData, Paths.Executable, () => new UserConfig { Version = UserConfig.CURRENT_VERSION });
            AppConfig.Manager = new ConfigManager<AppConfig>("appsettings.xml", Paths.Executable, null, () => new AppConfig { Version = AppConfig.CURRENT_VERSION });
        }
    }
}
