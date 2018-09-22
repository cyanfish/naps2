using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NAPS2.Util;

namespace NAPS2.ClientServer
{
    public class ClientContext : IDisposable
    {
        public IServerService Service { get; set; }

        public IServerCallback Callback { get; set; }

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
