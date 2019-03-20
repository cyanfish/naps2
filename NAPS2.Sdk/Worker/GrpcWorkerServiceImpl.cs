using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Scan;
using NAPS2.Scan.Twain;
using NAPS2.Scan.Wia;
using NAPS2.Scan.Wia.Native;
using NAPS2.Serialization;
using NAPS2.Util;

namespace NAPS2.Worker
{
    public class GrpcWorkerServiceImpl : GrpcWorkerService.GrpcWorkerServiceBase
    {
        private readonly ITwainWrapper twainWrapper;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly IMapiWrapper mapiWrapper;

        private CancellationTokenSource twainScanCts = new CancellationTokenSource();

        public GrpcWorkerServiceImpl(ITwainWrapper twainWrapper, ThumbnailRenderer thumbnailRenderer, IMapiWrapper mapiWrapper)
        {
            this.twainWrapper = twainWrapper;
            this.thumbnailRenderer = thumbnailRenderer;
            this.mapiWrapper = mapiWrapper;
        }

        public override Task<InitResponse> Init(InitRequest request, ServerCallContext context) =>
            GrpcHelper.WrapFunc(
                () =>
                {
                    if (!string.IsNullOrEmpty(request.RecoveryFolderPath))
                    {
                        FileStorageManager.Current = new RecoveryStorageManager(request.RecoveryFolderPath, true);
                    }
                    return new InitResponse();
                },
                err => new InitResponse { Error = err });

        public override Task<Wia10NativeUiResponse> Wia10NativeUi(Wia10NativeUiRequest request, ServerCallContext context) =>
            GrpcHelper.WrapFunc(
                () =>
                {
                    try
                    {
                        using (var deviceManager = new WiaDeviceManager(WiaVersion.Wia10))
                        using (var device = deviceManager.FindDevice(request.DeviceId))
                        {
                            var item = device.PromptToConfigure((IntPtr)request.Hwnd);
                            return new Wia10NativeUiResponse
                            {
                                WiaConfigurationXml = new WiaConfiguration
                                {
                                    DeviceProps = device.Properties.SerializeEditable(),
                                    ItemProps = item.Properties.SerializeEditable(),
                                    ItemName = item.Name()
                                }.ToXml()
                            };
                        }
                    }
                    catch (WiaException e)
                    {
                        WiaScanErrors.ThrowDeviceError(e);
                        throw new InvalidOperationException();
                    }
                },
                err => new Wia10NativeUiResponse { Error = err });

        public override Task<TwainGetDeviceListResponse> TwainGetDeviceList(TwainGetDeviceListRequest request, ServerCallContext context) =>
            GrpcHelper.WrapFunc(
                () => new TwainGetDeviceListResponse
                {
                    DeviceListXml = twainWrapper.GetDeviceList(request.TwainImpl.FromXml<TwainImpl>()).ToXml()
                },
                err => new TwainGetDeviceListResponse { Error = err });

        public override Task TwainScan(TwainScanRequest request, IServerStreamWriter<TwainScanResponse> responseStream, ServerCallContext context) =>
            GrpcHelper.WrapAction(
                () =>
                {
                    var imagePathDict = new Dictionary<ScannedImage, string>();
                    twainWrapper.Scan(
                        (IntPtr)request.Hwnd,
                        request.ScanDeviceXml.FromXml<ScanDevice>(),
                        request.ScanProfileXml.FromXml<ScanProfile>(),
                        request.ScanParamsXml.FromXml<ScanParams>(),
                        context.CancellationToken,
                        new WorkerImageSink(responseStream, imagePathDict),
                        (img, _, path) => imagePathDict.Add(img, path));
                }, err => responseStream.WriteAsync(new TwainScanResponse { Error = err }));

        public override Task<SendMapiEmailResponse> SendMapiEmail(SendMapiEmailRequest request, ServerCallContext context) =>
            GrpcHelper.WrapFunc(
                () => new SendMapiEmailResponse
                {
                    ReturnCodeXml = mapiWrapper.SendEmail(request.EmailMessageXml.FromXml<EmailMessage>()).ToXml()
                },
                err => new SendMapiEmailResponse { Error = err });

        public override Task<RenderThumbnailResponse> RenderThumbnail(RenderThumbnailRequest request, ServerCallContext context) =>
            GrpcHelper.WrapFunc(
                async () =>
                {
                    var deserializeOptions = new SerializedImageHelper.DeserializeOptions
                    {
                        ShareFileStorage = true
                    };
                    using (var image = SerializedImageHelper.Deserialize(request.Image, deserializeOptions))
                    {
                        var thumbnail = await thumbnailRenderer.Render(image, request.Size);
                        var stream = StorageManager.Convert<MemoryStreamStorage>(thumbnail, new StorageConvertParams { Lossless = true }).Stream;
                        return new RenderThumbnailResponse
                        {
                            Thumbnail = ByteString.FromStream(stream)
                        };
                    }
                },
                err => new RenderThumbnailResponse { Error = err });

        private class WorkerImageSink : ScannedImageSink
        {
            private readonly IServerStreamWriter<TwainScanResponse> callback;
            private readonly Dictionary<ScannedImage, string> imagePathDict;

            public WorkerImageSink(IServerStreamWriter<TwainScanResponse> callback, Dictionary<ScannedImage, string> imagePathDict)
            {
                this.callback = callback;
                this.imagePathDict = imagePathDict;
            }

            public override void PutImage(ScannedImage image)
            {
                // TODO: Ideally this shouldn't be inheriting ScannedImageSink, some other cleaner mechanism
                callback.WriteAsync(new TwainScanResponse
                {
                    Image = SerializedImageHelper.Serialize(image, new SerializedImageHelper.SerializeOptions
                    {
                        TransferOwnership = true,
                        IncludeThumbnail = true,
                        RenderedFilePath = imagePathDict.Get(image)
                    })
                }).Wait();
            }
        }
    }
}
