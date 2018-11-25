using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NAPS2.Util
{
    public static class ExceptionExtensions
    {
        private static MethodInfo internalPreserveStackTrace;

        static ExceptionExtensions()
        {
            internalPreserveStackTrace = typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Maintains the stack trace of an exception even after it is rethrown.
        /// This can be helpful when marshalling exceptions across process boundaries.
        /// </summary>
        /// <param name="e"></param>
        public static void PreserveStackTrace(this Exception e)
        {
            if (internalPreserveStackTrace != null)
            {
                internalPreserveStackTrace.Invoke(e, new object[] { });
            }
        }
    }
}
