using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Util
{
    /// <summary>
    /// Synchronized access to the UI thread.
    /// </summary>
    public static class Invoker
    {
        private static IInvoker _current = new DefaultInvoker();

        /// <summary>
        /// Gets or sets the current implementation of synchronized access to the UI thread.
        /// </summary>
        public static IInvoker Current
        {
            get => _current;
            set => _current = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
