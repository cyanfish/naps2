using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config
{
    public class StubConfigProvider<TConfig> : ConfigProvider<TConfig>
    {
        private readonly TConfig config;

        public StubConfigProvider(TConfig config)
        {
            this.config = config;
        }

        protected override T GetInternal<T>(Func<TConfig, T> func) => func(config);
    }
}
