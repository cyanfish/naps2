using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NAPS2.Scan;

namespace NAPS2.ClientServer
{
    [ServiceContract(CallbackContract = typeof(IServerCallback))]
    public interface IServerService
    {
        [OperationContract]
        List<string> GetSupportedDriverNames();

        [OperationContract]
        List<ScanDevice> GetDeviceList(string driverName);

        [OperationContract]
        void Scan(ScanProfile scanProfile, ScanParams scanParams);
    }
}
