using System.Linq.Expressions;

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
// TODO: A possible plan to get rid of the requirement that everything be nullable:
// TODO: - Create a KeyValueConfigScope that's functionally like ObjectConfigScope but can know whether a value is set
// TODO: - Use it (or a shared primitive) in FileConfigScope for local changes
// TODO: - Change GetInternal to TryGetInternal
// TODO: The problem with this still is that we're (de)serializing to a file using the type.
// TODO: We also did want to consider using the old appsettings format.
// TODO: So basically we can change the in-memory persistence to key-value easily, but doing that with the xml serializer is harder.
// TODO: Maybe the primitive can be the object plus a path set? Then for deserialization we pre-process the XDoc to get the path set
// TODO: And for serialization we post-process the XDocument to strip out everything not in the path set
// TODO: For lists/arrays, as usual the list/array itself is the end node. i.e. everything not marked as [Child] is an end node
// TODO: Though we have to consider how to handle xsi:null. Probably it's best to not treat null specially and conclude that anything present is to be considered specified. And null is a valid value for a pref that can be specified.
//
// TODO: We also would want special handling for getting/setting [Child] props that recursively enumerates over each subprop.
//
// TODO: We also need a way to unset properties (as opposed to setting null), as in FPdfSettings run scope management
// TODO: We would also need to get rid of SetAll, and consider on a case-by-case basis how to handle them.
// 
// TODO: And we definitely want some good tests for this.
public class ScopedConfig
{
    public static ScopedConfig Stub() =>
        new(
            ConfigScope.Memory<CommonConfig>(),
            ConfigScope.Memory<CommonConfig>(),
            ConfigScope.Memory<CommonConfig>(),
            ConfigScope.Memory<CommonConfig>(),
            ConfigScope.Defaults(InternalDefaults.GetCommonConfig()));

    private readonly ConfigScope<CommonConfig>[] _scopes;

    public ScopedConfig(string appConfigPath, string userConfigPath)
    {
        AppLocked = ConfigScope.File(appConfigPath, new ConfigSerializer(ConfigReadMode.LockedOnly), ConfigScopeMode.ReadOnly);
        Run = ConfigScope.Memory<CommonConfig>();
        User = ConfigScope.File(userConfigPath, new ConfigSerializer(ConfigReadMode.All), ConfigScopeMode.ReadWrite);
        AppDefault = ConfigScope.File(appConfigPath, new ConfigSerializer(ConfigReadMode.DefaultOnly), ConfigScopeMode.ReadOnly);
        InternalDefault = ConfigScope.Defaults(InternalDefaults.GetCommonConfig());

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

    public T Get<T>(Expression<Func<CommonConfig, T>> accessor)
    {
        foreach (var scope in _scopes)
        {
            if (scope.TryGet(accessor, out var value))
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