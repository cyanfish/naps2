using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config
{
    public interface IConfigManager<out T>
    {
        T Config { get; }
        void Load();
        void Save();
    }
}