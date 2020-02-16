using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config
{
    public class ScopeSetConfigProvider<TConfig> : ConfigProvider<TConfig>
    {
        private readonly ConfigScope<TConfig>[] _scopes;

        public ScopeSetConfigProvider(params ConfigScope<TConfig>[] scopes)
        {
            _scopes = scopes.ToArray();
        }

        public ScopeSetConfigProvider(IEnumerable<ConfigScope<TConfig>> scopes)
        {
            _scopes = scopes.ToArray();
        }

        protected override T GetInternal<T>(Func<TConfig, T> func)
        {
            foreach (var scope in _scopes)
            {
                var value = scope.Get(func);
                if (value != null)
                {
                    return value;
                }
            }
            // This shouldn't happen - the last config scope should always define a default value for every property.
            throw new Exception("Config value not defined.");
        }

        public ScopeSetConfigProvider<TConfig> Replace(ConfigScope<TConfig> original, ConfigScope<TConfig> replacement) =>
            new ScopeSetConfigProvider<TConfig>(_scopes.Select(x => x == original ? replacement : x));
    }
}
