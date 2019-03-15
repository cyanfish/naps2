using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config.Experimental
{
    public class ScopeSetConfigProvider<TConfig> : ConfigProvider<TConfig> where TConfig : new()
    {
        private readonly ConfigScope<TConfig>[] scopes;

        public ScopeSetConfigProvider(params ConfigScope<TConfig>[] scopes)
        {
            this.scopes = scopes.ToArray();
        }

        protected override T GetInternal<T>(Func<TConfig, T> func)
        {
            foreach (var scope in scopes)
            {
                var value = scope.Get(func);
                if (value != null)
                {
                    return value;
                }
            }
            // TODO: Consider throwing an exception
            return default;
        }
    }
}
