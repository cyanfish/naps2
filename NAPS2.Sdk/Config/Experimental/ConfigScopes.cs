using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.Config.Experimental
{
    public class ConfigScopes
    {
        private static ConfigScopes _current;

        public static ConfigScopes Current
        {
            get
            {
                TestingContext.NoStaticDefaults();
                if (_current == null)
                {
                    throw new InvalidOperationException("Config scopes have not been specified.");
                }
                return _current;
            }
            set => _current = value;
        }

        public ConfigScopes(string appConfigPath, string userConfigPath)
        {
            AppLocked = new FileConfigScope<CommonConfig>(appConfigPath, CommonConfig.Create, new ConfigSerializer(ConfigReadMode.LockedOnly), ConfigScopeMode.ReadOnly);
            Run = new ObjectConfigScope<CommonConfig>(new CommonConfig(), ConfigScopeMode.ReadWrite);
            User = new FileConfigScope<CommonConfig>(userConfigPath, CommonConfig.Create, new ConfigSerializer(ConfigReadMode.All), ConfigScopeMode.ReadWrite);
            AppDefault = new FileConfigScope<CommonConfig>(appConfigPath, CommonConfig.Create, new ConfigSerializer(ConfigReadMode.DefaultOnly), ConfigScopeMode.ReadOnly);
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
