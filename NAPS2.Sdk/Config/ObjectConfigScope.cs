using System;

namespace NAPS2.Config
{
    public class ObjectConfigScope<TConfig> : ConfigScope<TConfig>
    {
        private readonly TConfig _obj;

        public ObjectConfigScope(TConfig obj, ConfigScopeMode mode) : base(mode)
        {
            _obj = obj;
        }

        protected override T GetInternal<T>(Func<TConfig, T> func) => func(_obj);

        protected override void SetInternal(Action<TConfig> func) => func(_obj);

        protected override void SetAllInternal(TConfig delta)
        {
            ConfigCopier.Copy(delta, _obj);
        }
    }
}
