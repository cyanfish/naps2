using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Scan.Wia;
using NAPS2.Scan.Wia.Native;
using NAPS2.Serialization;

namespace NAPS2.Remoting.Worker
{
    public class WorkerServiceImpl : WorkerService.WorkerServiceBase
    {
        private readonly ImageContext imageContext;
        private readonly IRemoteScanController remoteScanController;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly IMapiWrapper mapiWrapper;

        public WorkerServiceImpl(ImageContext imageContext, ThumbnailRenderer thumbnailRenderer, IMapiWrapper mapiWrapper, BlankDetector blankDetector)
            : this(imageContext, new RemoteScanController(new ScanDriverFactory(imageContext), new RemotePostProcessor(imageContext, blankDetector)),
                thumbnailRenderer, mapiWrapper)
        {
        }

        internal WorkerServiceImpl(ImageContext imageContext, IRemoteScanController remoteScanController, ThumbnailRenderer thumbnailRenderer,
            IMapiWrapper mapiWrapper)
        {
            this.imageContext = imageContext;
            this.remoteScanController = remoteScanController;
            this.thumbnailRenderer = thumbnailRenderer;
            this.mapiWrapper = mapiWrapper;
        }

        public override Task<InitResponse> Init(InitRequest request, ServerCallContext context) =>
            RemotingHelper.WrapFunc(
                () =>
                {
                    if (!string.IsNullOrEmpty(request.RecoveryFolderPath))
                    {
                        imageContext.UseFileStorage(request.RecoveryFolderPath);
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
                        using var deviceManager = new WiaDeviceManager(WiaVersion.Wia10);
                        using var device = deviceManager.FindDevice(request.DeviceId);
                        var item = device.PromptToConfigure((IntPtr) request.Hwnd);
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
                    catch (WiaException e)
                    {
                        WiaScanErrors.ThrowDeviceError(e);
                        throw new InvalidOperationException();
                    }
                },
                err => new Wia10NativeUiResponse { Error = err });

        public override Task<GetDeviceListResponse> GetDeviceList(GetDeviceListRequest request, ServerCallContext context) =>
            RemotingHelper.WrapFunc(
                async () => new GetDeviceListResponse
                {
                    DeviceListXml = (await remoteScanController.GetDeviceList(request.OptionsXml.FromXml<ScanOptions>())).ToXml()
                },
                err => new GetDeviceListResponse { Error = err });

        public override Task Scan(ScanRequest request, IServerStreamWriter<ScanResponse> responseStream, ServerCallContext context)
        {
            // gRPC doesn't allow multiple pending writes. However, we don't want to write synchronously because that will slow down the scanning process.
            // Instead, we can use some async magic (chained tasks) to only write one at a time. 
            Task lastWriteTask = Task.CompletedTask;

            Task WriteSequenced(ScanResponse response)
            {
                lastWriteTask = lastWriteTask.ContinueWith(t => responseStream.WriteAsync(response)).Unwrap();
                return lastWriteTask;
            }

            return RemotingHelper.WrapAction(
                async () =>
                {
                    var scanEvents = new ScanEvents(
                        () => WriteSequenced(new ScanResponse
                        {
                            PageStart = new PageStartEvent()
                        }),
                        progress => WriteSequenced(new ScanResponse
                        {
                            Progress = new ProgressEvent
                            {
                                Value = progress
                            }
                        })
                    );
                    await remoteScanController.Scan(request.OptionsXml.FromXml<ScanOptions>(), context.CancellationToken, scanEvents,
                        (image, postProcessingContext) =>
                            WriteSequenced(new ScanResponse
                            {
                                Image = SerializedImageHelper.Serialize(imageContext, image, new SerializedImageHelper.SerializeOptions
                                {
                                    TransferOwnership = true,
                                    IncludeThumbnail = true,
                                    RenderedFilePath = postProcessingContext.TempPath
                                })
                            }));
                    // It's important that we wait for writes to complete, otherwise the channel might close first.
                    await lastWriteTask;
                }, err => WriteSequenced(new ScanResponse { Error = err }).Wait());
        }

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
                    using var image = SerializedImageHelper.Deserialize(imageContext, request.Image, deserializeOptions);
                    var thumbnail = await thumbnailRenderer.Render(image, request.Size);
                    var stream = imageContext.Convert<MemoryStreamStorage>(thumbnail, new StorageConvertParams { Lossless = true }).Stream;
                    return new RenderThumbnailResponse
                    {
                        Thumbnail = ByteString.FromStream(stream)
                    };
                },
                err => new RenderThumbnailResponse { Error = err });

        public override Task<RenderPdfResponse> RenderPdf(RenderPdfRequest request, ServerCallContext context) =>
            RemotingHelper.WrapFunc(
                () =>
                {
                    using var image = new PdfiumPdfRenderer(imageContext).Render(request.Path, request.Dpi).Single();
                    var stream = imageContext.Convert<MemoryStreamStorage>(image, new StorageConvertParams { Lossless = true }).Stream;
                    return new RenderPdfResponse
                    {
                        Image = ByteString.FromStream(stream)
                    };
                },
                err => new RenderPdfResponse { Error = err });
    }
}
