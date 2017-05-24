using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config
{
    public class AppConfigManager : ConfigManager<AppConfig>
    {
        public AppConfigManager()
            : base("appsettings.xml", Paths.Executable, null, () => new AppConfig { Version = AppConfig.CURRENT_VERSION })
        {
        }

        public new AppConfig Config => base.Config;
    }
}
