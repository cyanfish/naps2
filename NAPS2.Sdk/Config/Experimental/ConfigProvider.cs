using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config.Experimental
{
    // TODO: It is pretty ugly to have ConfigProvider<CommonConfig> everywhere.
    // TODO: Maybe a CommonConfigProvider subclass. Or maybe give in and remove generics.
    public abstract class ConfigProvider<TConfig>
    {
        public T Get<T>(Func<TConfig, T> func) => GetInternal(func);

        public T Get<T>(Func<TConfig, T?> func) where T : struct => GetInternal(func) ?? default;

        protected abstract T GetInternal<T>(Func<TConfig, T> func);
    }
}
