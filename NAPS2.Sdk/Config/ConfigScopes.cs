using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Util;

namespace NAPS2.Config
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

        public static ConfigScopes Stub() =>
            new ConfigScopes(
                ConfigScope.Object(new CommonConfig()),
                ConfigScope.Object(new CommonConfig()),
                ConfigScope.Object(new CommonConfig()),
                ConfigScope.Object(new CommonConfig()),
                ConfigScope.Object(InternalDefaults.GetCommonConfig()));

        public ConfigScopes(string appConfigPath, string userConfigPath)
        {
            AppLocked = ConfigScope.File(appConfigPath, CommonConfig.Create, new ConfigSerializer(ConfigReadMode.LockedOnly), ConfigScopeMode.ReadOnly);
            Run = ConfigScope.Object(new CommonConfig(), ConfigScopeMode.ReadWrite);
            User = ConfigScope.File(userConfigPath, CommonConfig.Create, new ConfigSerializer(ConfigReadMode.All), ConfigScopeMode.ReadWrite);
            AppDefault = ConfigScope.File(appConfigPath, CommonConfig.Create, new ConfigSerializer(ConfigReadMode.DefaultOnly), ConfigScopeMode.ReadOnly);
            InternalDefault = ConfigScope.Object(InternalDefaults.GetCommonConfig(), ConfigScopeMode.ReadOnly);

            Provider = ConfigProvider.Set(AppLocked, Run, User, AppDefault, InternalDefault);
        }

        public ConfigScopes(ConfigScope<CommonConfig> appLocked, ConfigScope<CommonConfig> run, ConfigScope<CommonConfig> user,
            ConfigScope<CommonConfig> appDefault, ConfigScope<CommonConfig> internalDefault)
        {
            AppLocked = appLocked;
            Run = run;
            User = user;
            AppDefault = appDefault;
            InternalDefault = internalDefault;

            Provider = ConfigProvider.Set(AppLocked, Run, User, AppDefault, InternalDefault);
        }

        public ScopeSetConfigProvider<CommonConfig> Provider { get; }

        public ConfigScope<CommonConfig> AppLocked { get; }
        public ConfigScope<CommonConfig> Run { get; }
        public ConfigScope<CommonConfig> User { get; }
        public ConfigScope<CommonConfig> AppDefault { get; }
        public ConfigScope<CommonConfig> InternalDefault { get; }
    }
}
