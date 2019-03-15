using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config.Experimental
{
    public abstract class ConfigProvider<TConfig> where TConfig : new()
    {
        public T Get<T>(Func<TConfig, T> func) where T : class => GetInternal(func);

        public T Get<T>(Func<TConfig, T?> func) where T : struct => GetInternal(func) ?? default;

        protected abstract T GetInternal<T>(Func<TConfig, T> func);
    }
}
