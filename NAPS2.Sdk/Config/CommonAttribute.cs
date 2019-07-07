using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NAPS2.Config
{
    public class CommonAttribute : ConfigPropAttribute
    {
        public CommonAttribute([CallerLineNumber] int line = 0) : base(line)
        {
        }
    }
}
