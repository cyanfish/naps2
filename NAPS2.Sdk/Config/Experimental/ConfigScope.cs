using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config.Experimental
{
    public abstract class ConfigScope
    {
        protected ConfigScope(ConfigScopeMode mode)
        {
            Mode = mode;
        }

        public ConfigScopeMode Mode { get; }

        public T Get<T>(Func<CommonConfig, T> func)
        {
            lock (this)
            {
                return GetInternal(func);
            }
        }

        public void Set(Action<CommonConfig> func)
        {
            if (Mode == ConfigScopeMode.ReadOnly)
            {
                throw new NotSupportedException("This config scope is in ReadOnly mode.");
            }
            lock (this)
            {
                SetInternal(func);
            }
        }

        public void SetAll(CommonConfig changes)
        {
            if (Mode == ConfigScopeMode.ReadOnly)
            {
                throw new NotSupportedException("This config scope is in ReadOnly mode.");
            }
            lock (this)
            {
                SetAllInternal(changes);
            }
        }

        protected abstract T GetInternal<T>(Func<CommonConfig, T> func);

        protected abstract void SetInternal(Action<CommonConfig> func);

        public abstract void SetAllInternal(CommonConfig delta);
    }
}
