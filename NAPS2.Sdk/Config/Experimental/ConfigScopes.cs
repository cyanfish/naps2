using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config.Experimental
{
    public class ConfigScopes
    {
        public ConfigScopes(string appConfigPath, string userConfigPath)
        {
            AppLocked = new FileConfigScope<CommonConfig>(appConfigPath, new ConfigSerializer(ConfigReadMode.LockedOnly), ConfigScopeMode.ReadOnly);
            Run = new ObjectConfigScope<CommonConfig>(new CommonConfig(), ConfigScopeMode.ReadWrite);
            User = new FileConfigScope<CommonConfig>(userConfigPath, new ConfigSerializer(ConfigReadMode.All), ConfigScopeMode.ReadWrite);
            AppDefault = new FileConfigScope<CommonConfig>(appConfigPath, new ConfigSerializer(ConfigReadMode.DefaultOnly), ConfigScopeMode.ReadOnly);
            InternalDefault = new ObjectConfigScope<CommonConfig>(InternalDefaults.GetCommonConfig(), ConfigScopeMode.ReadOnly);

            Provider = new ScopeSetConfigProvider<CommonConfig>(AppLocked, Run, User, AppDefault, InternalDefault);
        }

        public ConfigProvider<CommonConfig> Provider { get; }

        public ConfigScope<CommonConfig> AppLocked { get; }
        public ConfigScope<CommonConfig> Run { get; }
        public ConfigScope<CommonConfig> User { get; }
        public ConfigScope<CommonConfig> AppDefault { get; }
        public ConfigScope<CommonConfig> InternalDefault { get; }
    }
}
