using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config.Experimental
{
    public class ConfigScopes
    {
        public ConfigScopes(string appConfigPath, string userConfigPath)
        {
            AppLocked = new FileConfigScope(appConfigPath, new ConfigSerializer(ConfigReadMode.LockedOnly), ConfigScopeMode.ReadOnly);
            Run = new ObjectConfigScope(new CommonConfig(), ConfigScopeMode.ReadWrite);
            User = new FileConfigScope(userConfigPath, new ConfigSerializer(ConfigReadMode.All), ConfigScopeMode.ReadWrite);
            AppDefault = new FileConfigScope(appConfigPath, new ConfigSerializer(ConfigReadMode.DefaultOnly), ConfigScopeMode.ReadOnly);
            InternalDefault = new ObjectConfigScope(InternalDefaults.GetCommonConfig(), ConfigScopeMode.ReadOnly);

            Provider = new ScopeSetConfigProvider(AppLocked, Run, User, AppDefault, InternalDefault);
        }

        public ConfigProvider Provider { get; }

        public ConfigScope AppLocked { get; }
        public ConfigScope Run { get; }
        public ConfigScope User { get; }
        public ConfigScope AppDefault { get; }
        public ConfigScope InternalDefault { get; }
    }
}
