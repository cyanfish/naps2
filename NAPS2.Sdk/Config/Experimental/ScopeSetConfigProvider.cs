using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config.Experimental
{
    public class ScopeSetConfigProvider : ConfigProvider
    {
        private readonly ConfigScope[] scopes;

        public ScopeSetConfigProvider(params ConfigScope[] scopes)
        {
            this.scopes = scopes.ToArray();
        }

        protected override T GetInternal<T>(Func<CommonConfig, T> func)
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
