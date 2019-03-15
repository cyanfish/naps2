using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config.Experimental
{
    public class ObjectConfigScope : ConfigScope
    {
        private readonly CommonConfig obj;

        public ObjectConfigScope(CommonConfig obj, ConfigScopeMode mode) : base(mode)
        {
            this.obj = obj;
        }

        protected override T GetInternal<T>(Func<CommonConfig, T> func) => func(obj);

        protected override void SetInternal(Action<CommonConfig> func) => func(obj);

        public override void SetAllInternal(CommonConfig delta)
        {
            ConfigCopier.Copy(delta, obj);
        }
    }
}
