using System;
using System.Collections.Generic;
using System.Threading;
using NAPS2.Images;

namespace NAPS2.Scan.Twain
{
    public interface ITwainWrapper
    {
        List<ScanDevice> GetDeviceList(TwainImpl twainImpl);

        void Scan(IntPtr dialogParent, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams,
            CancellationToken cancelToken, ScannedImageSink sink, Action<ScannedImage, ScanParams, string> runBackgroundOcr);
    }
}
