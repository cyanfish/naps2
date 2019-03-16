using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using NAPS2.Images;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Scan;
using NAPS2.Scan.Twain;
using NAPS2.Scan.Wia;
using NAPS2.Util;

namespace NAPS2.Worker
{
    public class GrpcWorkerServiceImpl : GrpcWorkerService.GrpcWorkerServiceBase
    {
        private readonly TwainWrapper twainWrapper;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly MapiWrapper mapiWrapper;

        private CancellationTokenSource twainScanCts = new CancellationTokenSource();

        public GrpcWorkerServiceImpl(TwainWrapper twainWrapper, ThumbnailRenderer thumbnailRenderer, MapiWrapper mapiWrapper)
        {
            this.twainWrapper = twainWrapper;
            this.thumbnailRenderer = thumbnailRenderer;
            this.mapiWrapper = mapiWrapper;
        }

        public override async Task<TwainGetDeviceListResponse> TwainGetDeviceList(TwainGetDeviceListRequest request, ServerCallContext context)
        {
            return await Task.Run(() => new TwainGetDeviceListResponse
            {
                DeviceListXml = twainWrapper.GetDeviceList(request.TwainImpl.FromXml<TwainImpl>()).ToXml()
            });
        }
    }
}