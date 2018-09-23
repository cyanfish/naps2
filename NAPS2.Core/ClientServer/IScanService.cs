using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using NAPS2.Scan;

namespace NAPS2.ClientServer
{
    [ServiceContract(CallbackContract = typeof(IScanCallback))]
    public interface IScanService
    {
        [OperationContract]
        List<string> GetSupportedDriverNames();

        [OperationContract]
        List<ScanDevice> GetDeviceList(string driverName);

        [OperationContract]
        Task Scan(ScanProfile scanProfile, ScanParams scanParams);
    }
}
