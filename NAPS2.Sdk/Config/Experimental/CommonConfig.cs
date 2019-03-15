using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config.Experimental
{
    public class CommonConfig
    {
        public bool? SingleInstance { get; set; }

        public string Culture { get; set; }

        public NestedConfig Nested { get; set; }
    }

    public class NestedConfig
    {
        public int? SomeInt { get; set; }
    }
}
