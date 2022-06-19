namespace NAPS2.Config;

/// <summary>
/// Represents a source of configuration data. The source must be complete, i.e. it must never return null.
/// </summary>
/// <typeparam name="TConfig"></typeparam>
public interface IConfigProvider<TConfig>
{
    public T Get<T>(Func<TConfig, T?> func) where T : class;

    public T Get<T>(Func<TConfig, T?> func) where T : struct;
}