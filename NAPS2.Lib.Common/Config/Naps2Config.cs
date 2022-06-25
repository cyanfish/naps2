using NAPS2.Config.Model;

namespace NAPS2.Config;

/// <summary>
/// Represents the full NAPS2 configuration. Configuration sources are divided into several scopes:<br/>
/// - AppLocked: settings that are locked to a particular value by the administrator<br/>
/// - Run: settings set by the user that will expire after the application is closed<br/>
/// - User: settings set by the user that will persist after the application is closed<br/>
/// - AppDefault: default values for settings provided by the administrator<br/>
/// - InternalDefault: fallback values in case the setting isn't provided by any other source<br/>
///
/// Getting a config value will automatically enumerate the scopes in order and return the first value.
/// Setting a config value must be done on one of the writable scopes (Run or User).
///
/// A Naps2Config object can also represent a transaction, where the Run or User scopes contain uncommitted values.
/// </summary>
public class Naps2Config : ScopedConfig<CommonConfig>
{
    public static Naps2Config Stub() =>
        new(
            ConfigScope.Memory<CommonConfig>(),
            ConfigScope.Memory<CommonConfig>(),
            ConfigScope.Memory<CommonConfig>(),
            ConfigScope.Memory<CommonConfig>(),
            ConfigScope.Defaults(InternalDefaults.GetCommonConfig()));

    public Naps2Config(string appConfigPath, string userConfigPath)
    {
        AppLocked = ConfigScope.File(appConfigPath, new ConfigSerializer(ConfigReadMode.LockedOnly, ConfigRootName.AppConfig),
            ConfigScopeMode.ReadOnly);
        Run = ConfigScope.Memory<CommonConfig>();
        User = ConfigScope.File(userConfigPath, new ConfigSerializer(ConfigReadMode.All, ConfigRootName.UserConfig), ConfigScopeMode.ReadWrite);
        AppDefault = ConfigScope.File(appConfigPath, new ConfigSerializer(ConfigReadMode.DefaultOnly, ConfigRootName.AppConfig),
            ConfigScopeMode.ReadOnly);
        InternalDefault = ConfigScope.Defaults(InternalDefaults.GetCommonConfig());

        Scopes = new[] { AppLocked, Run, User, AppDefault, InternalDefault };
    }

    public Naps2Config(ConfigScope<CommonConfig> appLocked, ConfigScope<CommonConfig> run,
        ConfigScope<CommonConfig> user,
        ConfigScope<CommonConfig> appDefault, ConfigScope<CommonConfig> internalDefault)
    {
        AppLocked = appLocked;
        Run = run;
        User = user;
        AppDefault = appDefault;
        InternalDefault = internalDefault;

        Scopes = new[] { AppLocked, Run, User, AppDefault, InternalDefault };
    }

    // TODO: Maybe generalize this somehow
    public Naps2Config WithTransaction(params TransactionConfigScope<CommonConfig>[] transactions)
    {
        var userScope = User;
        var runScope = Run;
        foreach (var transaction in transactions)
        {
            if (transaction.OriginalScope == User)
            {
                userScope = transaction;
            }
            else if (transaction.OriginalScope == Run)
            {
                runScope = transaction;
            }
            else
            {
                throw new ArgumentException("Unsupported transaction scope");
            }
        }
        return new Naps2Config(AppLocked, runScope, userScope, AppDefault, InternalDefault);
    }

    public ConfigScope<CommonConfig> AppLocked { get; }
    public ConfigScope<CommonConfig> Run { get; }
    public ConfigScope<CommonConfig> User { get; }
    public ConfigScope<CommonConfig> AppDefault { get; }
    public ConfigScope<CommonConfig> InternalDefault { get; }
}