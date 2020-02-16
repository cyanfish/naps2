using System;
using System.Diagnostics;
using NAPS2.Logging;

namespace NAPS2.Remoting.Worker
{
    /// <summary>
    /// A class storing the objects the client needs to use a NAPS2.Worker.exe instance.
    /// </summary>
    public class WorkerContext : IDisposable
    {
        public WorkerContext(WorkerServiceAdapter service, Process process)
        {
            Service = service;
            Process = process;
        }
        
        public WorkerServiceAdapter Service { get; }

        public Process Process { get; }

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
