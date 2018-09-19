using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Util
{
    /// <summary>
    /// A default implementation for synchronized access to the UI thread that assumes there is no privileged thread.
    /// </summary>
    public class DefaultInvoker : IInvoker
    {
        public void Invoke(Action action) => action();

        public void SafeInvoke(Action action) => action();

        public T InvokeGet<T>(Func<T> func) => func();
    }
}
