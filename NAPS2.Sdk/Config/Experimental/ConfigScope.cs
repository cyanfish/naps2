using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config.Experimental
{
    public abstract class ConfigScope<TConfig> where TConfig : new()
    {
        protected ConfigScope(ConfigScopeMode mode)
        {
            Mode = mode;
        }

        public ConfigScopeMode Mode { get; }

        public T Get<T>(Func<TConfig, T> func)
        {
            lock (this)
            {
                return GetInternal(func);
            }
        }

        public void Set(Action<TConfig> func)
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

        public void SetAll(TConfig changes)
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

        protected abstract T GetInternal<T>(Func<TConfig, T> func);

        protected abstract void SetInternal(Action<TConfig> func);

        public abstract void SetAllInternal(TConfig delta);
    }
}
