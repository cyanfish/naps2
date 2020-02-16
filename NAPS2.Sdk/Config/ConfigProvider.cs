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
        [return: NotNull]
        public T Get<T>(Func<TConfig, T> func) => GetInternal(func);

        public T Get<T>(Func<TConfig, T?> func) where T : struct => GetInternal(func) ?? default;

        protected abstract T GetInternal<T>(Func<TConfig, T> func);
    }
}
