using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config.Experimental
{
    public abstract class ConfigPropAttribute : Attribute
    {
        protected ConfigPropAttribute(int line)
        {
            Line = line;
        }

        public int Line { get; }
    }
}
