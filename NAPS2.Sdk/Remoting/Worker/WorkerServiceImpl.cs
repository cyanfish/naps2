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
using NAPS2.Wia;
using NAPS2.Serialization;

namespace NAPS2.Remoting.Worker
{
    public class WorkerServiceImpl : WorkerService.WorkerServiceBase
    {
        private readonly ImageContext imageContext;
        private readonly IRemoteScanController remoteScanController;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly IMapiWrapper mapiWrapper;

        public WorkerServiceImpl(ImageContext imageContext, ThumbnailRenderer thumbnailRenderer, IMapiWrapper mapiWrapper)
            : this(imageContext, new RemoteScanController(imageContext),
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

        public override Task<InitResponse> Init(InitRequest request, ServerCallContext context)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.RecoveryFolderPath))
                {
                    imageContext.UseFileStorage(request.RecoveryFolderPath);
                }

                return Task.FromResult(new InitResponse());
            }
            catch (Exception e)
            {
                return Task.FromResult(new InitResponse { Error = RemotingHelper.ToError(e) });
            }
        }

        public override Task<Wia10NativeUiResponse> Wia10NativeUi(Wia10NativeUiRequest request, ServerCallContext context)
        {
            try
            {
                try
                {
                    using var deviceManager = new WiaDeviceManager(WiaVersion.Wia10);
                    using var device = deviceManager.FindDevice(request.DeviceId);
                    var item = device.PromptToConfigure((IntPtr) request.Hwnd);
                    return Task.FromResult(new Wia10NativeUiResponse
                    {
                        WiaConfigurationXml = new WiaConfiguration
                        {
                            DeviceProps = device.Properties.SerializeEditable(),
                            ItemProps = item.Properties.SerializeEditable(),
                            ItemName = item.Name()
                        }.ToXml()
                    });
                }
                catch (WiaException e)
                {
                    WiaScanErrors.ThrowDeviceError(e);
                    throw new InvalidOperationException();
                }
            }
            catch (Exception e)
            {
                return Task.FromResult(new Wia10NativeUiResponse { Error = RemotingHelper.ToError(e) });
            }
        }

        public override async Task<GetDeviceListResponse> GetDeviceList(GetDeviceListRequest request, ServerCallContext context)
        {
            try
            {
                var scanOptions = request.OptionsXml.FromXml<ScanOptions>();
                var deviceList = await remoteScanController.GetDeviceList(scanOptions);
                return new GetDeviceListResponse
                {
                    DeviceListXml = deviceList.ToXml()
                };
            }
            catch (Exception e)
            {
                return new GetDeviceListResponse { Error = RemotingHelper.ToError(e) };
            }
        }

        public override async Task Scan(ScanRequest request, IServerStreamWriter<ScanResponse> responseStream, ServerCallContext context)
        {
            var sequencedWriter = new SequencedWriter<ScanResponse>(responseStream);
            try
            {
                var scanEvents = new ScanEvents(
                    () => sequencedWriter.Write(new ScanResponse
                    {
                        PageStart = new PageStartEvent()
                    }),
                    progress => sequencedWriter.Write(new ScanResponse
                    {
                        Progress = new ProgressEvent
                        {
                            Value = progress
                        }
                    })
                );
                await remoteScanController.Scan(request.OptionsXml.FromXml<ScanOptions>(),
                    context.CancellationToken, scanEvents,
                    (image, postProcessingContext) =>
                        sequencedWriter.Write(new ScanResponse
                        {
                            Image = SerializedImageHelper.Serialize(imageContext, image,
                                new SerializedImageHelper.SerializeOptions
                                {
                                    TransferOwnership = true,
                                    IncludeThumbnail = true,
                                    RenderedFilePath = postProcessingContext.TempPath
                                })
                        }));
            }
            catch (Exception e)
            {
                sequencedWriter.Write(new ScanResponse { Error = RemotingHelper.ToError(e) });
            }
            await sequencedWriter.WaitForCompletion();
        }

        public override async Task<SendMapiEmailResponse> SendMapiEmail(SendMapiEmailRequest request, ServerCallContext context)
        {
            try
            {
                var emailMessage = request.EmailMessageXml.FromXml<EmailMessage>();
                var returnCode = await mapiWrapper.SendEmail(emailMessage);
                return new SendMapiEmailResponse
                {
                    ReturnCodeXml = returnCode.ToXml()
                };
            }
            catch (Exception e)
            {
                return new SendMapiEmailResponse { Error = RemotingHelper.ToError(e) };
            }
        }

        public override async Task<RenderThumbnailResponse> RenderThumbnail(RenderThumbnailRequest request, ServerCallContext context)
        {
            try
            {
                var deserializeOptions = new SerializedImageHelper.DeserializeOptions
                {
                    ShareFileStorage = true
                };
                using var image =
                    SerializedImageHelper.Deserialize(imageContext, request.Image, deserializeOptions);
                var thumbnail = await thumbnailRenderer.Render(image, request.Size);
                var stream = imageContext
                    .Convert<MemoryStreamStorage>(thumbnail, new StorageConvertParams {Lossless = true}).Stream;
                return new RenderThumbnailResponse
                {
                    Thumbnail = ByteString.FromStream(stream)
                };
            }
            catch (Exception e)
            {
                return new RenderThumbnailResponse { Error = RemotingHelper.ToError(e) };
            }
        }

        public override Task<RenderPdfResponse> RenderPdf(RenderPdfRequest request, ServerCallContext context)
        {
            try
            {
                var renderer = new PdfiumPdfRenderer(imageContext);
                using var image = renderer.Render(request.Path, request.Dpi).Single();
                var stream = imageContext
                    .Convert<MemoryStreamStorage>(image, new StorageConvertParams {Lossless = true}).Stream;
                return Task.FromResult(new RenderPdfResponse
                {
                    Image = ByteString.FromStream(stream)
                });
            }
            catch (Exception e)
            {
                return Task.FromResult(new RenderPdfResponse { Error = RemotingHelper.ToError(e) });
            }
        }
    }
}
