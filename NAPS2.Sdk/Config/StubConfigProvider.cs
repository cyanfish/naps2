using System;

namespace NAPS2.Config
{
    public class StubConfigProvider<TConfig> : ConfigProvider<TConfig>
    {
        private readonly TConfig _config;

        public StubConfigProvider(TConfig config)
        {
            _config = config;
        }

        protected override T GetInternal<T>(Func<TConfig, T> func) => func(_config);
    }
}
