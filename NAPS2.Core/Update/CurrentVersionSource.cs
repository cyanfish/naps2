using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NAPS2.Update
{
    public class CurrentVersionSource : ICurrentVersionSource
    {
        public Version GetCurrentVersion()
        {
            return Assembly.GetAssembly(typeof(CurrentVersionSource)).GetName().Version;
        }
    }
}