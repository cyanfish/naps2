using System;

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
/// A ScopedConfig object can also represent a transaction, where the Run or User scopes contain uncommitted values.
/// </summary>
public class ScopedConfig : IConfigProvider<CommonConfig>
{
    public static ScopedConfig Stub() =>
        new ScopedConfig(
            ConfigScope.Object(new CommonConfig()),
            ConfigScope.Object(new CommonConfig()),
            ConfigScope.Object(new CommonConfig()),
            ConfigScope.Object(new CommonConfig()),
            ConfigScope.Object(InternalDefaults.GetCommonConfig()));

    private readonly ConfigScope<CommonConfig>[] _scopes;

    public ScopedConfig(string appConfigPath, string userConfigPath)
    {
        AppLocked = ConfigScope.File(appConfigPath, CommonConfig.Create, new ConfigSerializer(ConfigReadMode.LockedOnly), ConfigScopeMode.ReadOnly);
        Run = ConfigScope.Object(new CommonConfig(), ConfigScopeMode.ReadWrite);
        User = ConfigScope.File(userConfigPath, CommonConfig.Create, new ConfigSerializer(ConfigReadMode.All), ConfigScopeMode.ReadWrite);
        AppDefault = ConfigScope.File(appConfigPath, CommonConfig.Create, new ConfigSerializer(ConfigReadMode.DefaultOnly), ConfigScopeMode.ReadOnly);
        InternalDefault = ConfigScope.Object(InternalDefaults.GetCommonConfig(), ConfigScopeMode.ReadOnly);

        _scopes = new[] { AppLocked, Run, User, AppDefault, InternalDefault };
    }

    public ScopedConfig(ConfigScope<CommonConfig> appLocked, ConfigScope<CommonConfig> run, ConfigScope<CommonConfig> user,
        ConfigScope<CommonConfig> appDefault, ConfigScope<CommonConfig> internalDefault)
    {
        AppLocked = appLocked;
        Run = run;
        User = user;
        AppDefault = appDefault;
        InternalDefault = internalDefault;

        _scopes = new[] { AppLocked, Run, User, AppDefault, InternalDefault };
    }

    public T Get<T>(Func<CommonConfig, T?> func) where T : struct
    {
        foreach (var scope in _scopes)
        {
            var nullable = scope.Get(func);
            if (nullable.HasValue)
            {
                return nullable.Value;
            }
        }
        // This shouldn't happen - the last config scope should always define a default value for every property.
        throw new Exception("Config value not defined.");
    }

    public T Get<T>(Func<CommonConfig, T?> func) where T : class
    {
        foreach (var scope in _scopes)
        {
            var value = scope.Get(func);
            if (value != null)
            {
                return value;
            }
        }
        // This shouldn't happen - the last config scope should always define a default value for every property.
        throw new Exception("Config value not defined.");
    }

    public ScopedConfig WithTransaction(params TransactionConfigScope<CommonConfig>[] transactions)
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
        return new ScopedConfig(AppLocked, runScope, userScope, AppDefault, InternalDefault);
    }

    public ConfigScope<CommonConfig> AppLocked { get; }
    public ConfigScope<CommonConfig> Run { get; }
    public ConfigScope<CommonConfig> User { get; }
    public ConfigScope<CommonConfig> AppDefault { get; }
    public ConfigScope<CommonConfig> InternalDefault { get; }
}