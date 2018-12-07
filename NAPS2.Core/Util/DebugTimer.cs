using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NAPS2.Util
{
    public class DebugTimer : IDisposable
    {
        private readonly string label;
        private readonly Stopwatch stopwatch;

        public DebugTimer(string label = null)
        {
            this.label = label;
            stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            Debug.WriteLine(label == null
                ? $"{stopwatch.ElapsedMilliseconds} ms"
                : $"{stopwatch.ElapsedMilliseconds} ms : {label}");
        }
    }
}
