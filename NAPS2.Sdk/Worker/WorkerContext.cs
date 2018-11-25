using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using NAPS2.Logging;
using NAPS2.Util;

namespace NAPS2.Worker
{
    /// <summary>
    /// A class storing the objects the client needs to use a NAPS2.Worker.exe instance.
    /// </summary>
    public class WorkerContext : IDisposable
    {
        public IWorkerService Service { get; set; }

        public IWorkerCallback Callback { get; set; }

        public Process Process { get; set; }

        public void Dispose()
        {
            try
            {
                ((IDisposable) Service)?.Dispose();
            }
            catch (CommunicationObjectFaultedException)
            {
            }
            catch (Exception e)
            {
                Log.ErrorException("Error cleaning up worker", e);
            }
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
