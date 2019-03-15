using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config.Experimental
{
    public class InternalDefaults
    {
        // TODO: Test that no properties are null
        public static CommonConfig GetCommonConfig() =>
            new CommonConfig
            {
                SingleInstance = false,
                Culture = "en",
                Nested = new NestedConfig
                {
                    SomeInt = 0
                }
            };
    }
}