using System;
using System.ServiceModel;
using NAPS2.Logging;

namespace NAPS2.Remoting.ClientServer
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
