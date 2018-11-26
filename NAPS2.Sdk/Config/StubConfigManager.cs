using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config
{
    public class StubConfigManager<T> : IConfigManager<T>
    {
        public StubConfigManager(T config)
        {
            Config = config;
        }

        public T Config { get; }

        public void Load()
        {
        }

        public void Save()
        {
        }
    }
}
