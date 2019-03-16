using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using NAPS2.Images;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Scan;
using NAPS2.Scan.Wia;
using NAPS2.Util;

namespace NAPS2.Worker
{
    class GrpcWorkerServiceAdapter : IWorkerService
    {
        private int port;

        public GrpcWorkerServiceAdapter(int port)
        {
            this.port = port;
        }

        public WiaConfiguration Wia10NativeUI(string scanDevice, IntPtr hwnd) => throw new NotImplementedException();

        public List<ScanDevice> TwainGetDeviceList(TwainImpl twainImpl)
        {
            var client = new GrpcWorkerService.GrpcWorkerServiceClient(new Channel("localhost", port, ChannelCredentials.Insecure));
            var response = client.TwainGetDeviceList(new TwainGetDeviceListRequest { TwainImpl = twainImpl.ToXml() });
            return response.DeviceListXml.FromXml<List<ScanDevice>>();
        }

        public Task TwainScan(ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, IntPtr hwnd) => throw new NotImplementedException();

        public void CancelTwainScan()
        {
            throw new NotImplementedException();
        }

        public MapiSendMailReturnCode SendMapiEmail(EmailMessage message) => throw new NotImplementedException();

        public byte[] RenderThumbnail(ScannedImage.Snapshot snapshot, int size) => throw new NotImplementedException();

        public void Init(string recoveryFolderPath)
        {
            //throw new NotImplementedException();
        }
    }
}