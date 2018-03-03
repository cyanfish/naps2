using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NAPS2.Util
{
    public static class ExceptionExtensions
    {
        private static MethodInfo internalPreserveStackTrace;

        static ExceptionExtensions()
        {
            internalPreserveStackTrace = typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static void PreserveStackTrace(this Exception e)
        {
            if (internalPreserveStackTrace != null)
            {
                internalPreserveStackTrace.Invoke(e, new object[] { });
            }
        }
    }
}
