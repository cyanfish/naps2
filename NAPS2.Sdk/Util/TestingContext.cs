using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NAPS2.Util
{
    /// <summary>
    /// Provides facilities to detect unit testing and throw exceptions for bad testing practices.
    /// </summary>
    public static class TestingContext
    {
        public static bool IsTesting => Assembly.GetEntryAssembly() == null;

        /// <summary>
        /// If testing, throws an exception.
        /// </summary>
        public static void NoStaticDefaults()
        {
            if (IsTesting)
            {
                throw new InvalidOperationException("Static defaults should not be used for unit tests. Use constructor injection instead.");
            }
        }
    }
}
