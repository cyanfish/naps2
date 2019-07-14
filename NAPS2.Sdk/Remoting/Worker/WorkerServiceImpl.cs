using System;
using System.Collections.Generic;
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

namespace NAPS2.Remoting.Worker
{
    public class WorkerServiceImpl : WorkerService.WorkerServiceBase
    {
        private readonly ImageContext imageContext;
        private readonly ITwainWrapper twainWrapper;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly IMapiWrapper mapiWrapper;

        public WorkerServiceImpl(ImageContext imageContext, ITwainWrapper twainWrapper, ThumbnailRenderer thumbnailRenderer, IMapiWrapper mapiWrapper)
        {
            this.imageContext = imageContext;
            this.twainWrapper = twainWrapper;
            this.thumbnailRenderer = thumbnailRenderer;
            this.mapiWrapper = mapiWrapper;
        }

        public override Task<InitResponse> Init(InitRequest request, ServerCallContext context) =>
            RemotingHelper.WrapFunc(
                () =>
                {
                    if (!string.IsNullOrEmpty(request.RecoveryFolderPath))
                    {
                        imageContext.FileStorageManager = new RecoveryStorageManager(request.RecoveryFolderPath, true);
                    }
                    return new InitResponse();
                },
                err => new InitResponse { Error = err });

        public override Task<Wia10NativeUiResponse> Wia10NativeUi(Wia10NativeUiRequest request, ServerCallContext context) =>
            RemotingHelper.WrapFunc(
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
            RemotingHelper.WrapFunc(
                () => new TwainGetDeviceListResponse
                {
                    DeviceListXml = twainWrapper.GetDeviceList(request.TwainImpl.FromXml<TwainImpl>()).ToXml()
                },
                err => new TwainGetDeviceListResponse { Error = err });

        public override Task TwainScan(TwainScanRequest request, IServerStreamWriter<TwainScanResponse> responseStream, ServerCallContext context) =>
            RemotingHelper.WrapAction(
                () =>
                {
                    var imagePathDict = new Dictionary<ScannedImage, string>();
                    twainWrapper.Scan(
                        (IntPtr)request.Hwnd,
                        request.ScanDeviceXml.FromXml<ScanDevice>(),
                        request.ScanProfileXml.FromXml<ScanProfile>(),
                        request.ScanParamsXml.FromXml<ScanParams>(),
                        context.CancellationToken,
                        new WorkerImageSink(imageContext, responseStream, imagePathDict),
                        (img, _, path) => imagePathDict.Add(img, path));
                }, err => responseStream.WriteAsync(new TwainScanResponse { Error = err }));

        public override Task<SendMapiEmailResponse> SendMapiEmail(SendMapiEmailRequest request, ServerCallContext context) =>
            RemotingHelper.WrapFunc(
                () => new SendMapiEmailResponse
                {
                    ReturnCodeXml = mapiWrapper.SendEmail(request.EmailMessageXml.FromXml<EmailMessage>()).ToXml()
                },
                err => new SendMapiEmailResponse { Error = err });

        public override Task<RenderThumbnailResponse> RenderThumbnail(RenderThumbnailRequest request, ServerCallContext context) =>
            RemotingHelper.WrapFunc(
                async () =>
                {
                    var deserializeOptions = new SerializedImageHelper.DeserializeOptions
                    {
                        ShareFileStorage = true
                    };
                    using (var image = SerializedImageHelper.Deserialize(imageContext, request.Image, deserializeOptions))
                    {
                        var thumbnail = await thumbnailRenderer.Render(image, request.Size);
                        var stream = imageContext.Convert<MemoryStreamStorage>(thumbnail, new StorageConvertParams { Lossless = true }).Stream;
                        return new RenderThumbnailResponse
                        {
                            Thumbnail = ByteString.FromStream(stream)
                        };
                    }
                },
                err => new RenderThumbnailResponse { Error = err });

        private class WorkerImageSink : ScannedImageSink
        {
            private readonly ImageContext imageContext;
            private readonly IServerStreamWriter<TwainScanResponse> callback;
            private readonly Dictionary<ScannedImage, string> imagePathDict;

            public WorkerImageSink(ImageContext imageContext, IServerStreamWriter<TwainScanResponse> callback, Dictionary<ScannedImage, string> imagePathDict)
            {
                this.imageContext = imageContext;
                this.callback = callback;
                this.imagePathDict = imagePathDict;
            }

            public override void PutImage(ScannedImage image)
            {
                // TODO: Ideally this shouldn't be inheriting ScannedImageSink, some other cleaner mechanism
                callback.WriteAsync(new TwainScanResponse
                {
                    Image = SerializedImageHelper.Serialize(imageContext, image, new SerializedImageHelper.SerializeOptions
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
