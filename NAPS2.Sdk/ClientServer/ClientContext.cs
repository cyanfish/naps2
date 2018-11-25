using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NAPS2.Logging;
using NAPS2.Util;

namespace NAPS2.ClientServer
{
    public class ClientContext : IDisposable
    {
        public IScanService Service { get; set; }

        public IScanCallback Callback { get; set; }

        public void Dispose()
        {
            try
            {
                ((IDisposable)Service)?.Dispose();
            }
            catch (CommunicationObjectFaultedException)
            {
            }
            catch (Exception e)
            {
                Log.ErrorException("Error cleaning up client", e);
            }
        }
    }
}
