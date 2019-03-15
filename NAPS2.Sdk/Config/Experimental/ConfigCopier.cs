using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Config.Experimental
{
    public static class ConfigCopier
    {
        public static void Copy<T>(T src, T dst)
        {
            // TODO
            throw new NotImplementedException();
        }

        public static T Copy<T>(T src) where T : new()
        {
            var copy = new T();
            Copy(src, copy);
            return copy;
        }
    }
}
