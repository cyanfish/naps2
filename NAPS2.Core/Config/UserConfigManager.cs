using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config
{
    public class UserConfigManager : ConfigManager<UserConfig>, IUserConfigManager
    {
        public UserConfigManager()
            : base("config.xml", Paths.AppData, Paths.Executable, () => new UserConfig { Version = UserConfig.CURRENT_VERSION })
        {
        }

        public new UserConfig Config => base.Config;
    }
}
