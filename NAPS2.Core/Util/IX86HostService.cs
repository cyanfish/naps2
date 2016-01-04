using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using NAPS2.Scan;
using NAPS2.Scan.Images;

namespace NAPS2.Util
{
    [ServiceContract]
    public interface IX86HostService
    {
        [OperationContract]
        void DoWork();

        [OperationContract]
        ScanDevice TwainPromptForDevice();

        [OperationContract]
        List<IScannedImage> TwainScan(ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams);
    }
}
