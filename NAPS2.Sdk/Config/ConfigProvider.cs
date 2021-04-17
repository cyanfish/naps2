using System;
using System.Diagnostics.CodeAnalysis;

namespace NAPS2.Config
{
    public static class ConfigProvider
    {
        public static StubConfigProvider<T> Stub<T>(T value) =>
            new StubConfigProvider<T>(value);

        public static ScopeSetConfigProvider<T> Set<T>(params ConfigScope<T>[] scopes) =>
            new ScopeSetConfigProvider<T>(scopes);
    }

    public abstract class ConfigProvider<TConfig>
    {
        // TODO: This may actually return null. Specifically the scope set provider with a default config shouldn't. But the current model can't represent that.
        [return: NotNull]
        public T Get<T>(Func<TConfig, T> func) => GetInternal(func);

        public T Get<T>(Func<TConfig, T?> func) where T : struct => GetInternal(func) ?? default;

        protected abstract T GetInternal<T>(Func<TConfig, T> func);
    }
}
