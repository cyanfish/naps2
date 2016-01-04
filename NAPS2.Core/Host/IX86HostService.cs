using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NAPS2.Scan;
using NAPS2.Scan.Images;

namespace NAPS2.Host
{
    [ServiceContract]
    public interface IX86HostService
    {
        [OperationContract]
        void DoWork();

        [OperationContract]
        ScanDevice TwainPromptForDevice(IntPtr hwnd);

        [OperationContract]
        List<IScannedImage> TwainScan(IntPtr hwnd, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams);
    }
}
