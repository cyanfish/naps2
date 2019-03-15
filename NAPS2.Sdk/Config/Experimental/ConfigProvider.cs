using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config.Experimental
{
    public abstract class ConfigProvider
    {
        public T Get<T>(Func<CommonConfig, T> func) where T : class => GetInternal(func);

        public T Get<T>(Func<CommonConfig, T?> func) where T : struct => GetInternal(func) ?? default;

        protected abstract T GetInternal<T>(Func<CommonConfig, T> func);
    }
}
