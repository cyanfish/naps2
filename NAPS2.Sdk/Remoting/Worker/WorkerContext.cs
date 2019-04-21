using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NAPS2.Logging;

namespace NAPS2.Remoting.Worker
{
    /// <summary>
    /// A class storing the objects the client needs to use a NAPS2.Worker.exe instance.
    /// </summary>
    public class WorkerContext : IDisposable
    {
        public WorkerServiceAdapter Service { get; set; }

        public Process Process { get; set; }

        public void Dispose()
        {
            try
            {
                Process.Kill();
            }
            catch (Exception e)
            {
                Log.ErrorException("Error cleaning up worker", e);
            }
        }
    }
}
