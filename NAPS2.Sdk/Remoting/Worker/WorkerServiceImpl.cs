using System.Threading;
using Google.Protobuf;
using Grpc.Core;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Scan.Internal.Twain;
using NAPS2.Scan.Wia;
using NAPS2.Wia;
using NAPS2.Serialization;

namespace NAPS2.Remoting.Worker;

public class WorkerServiceImpl : WorkerService.WorkerServiceBase
{
    private readonly ScanningContext _scanningContext;
    private readonly IRemoteScanController _remoteScanController;
    private readonly ThumbnailRenderer _thumbnailRenderer;
    private readonly IMapiWrapper _mapiWrapper;
    private readonly ITwainController _twainController;

    private readonly AutoResetEvent _ongoingCallFinished = new(false);
    private int _ongoingCallCount;

    public WorkerServiceImpl(ScanningContext scanningContext, ThumbnailRenderer thumbnailRenderer,
        IMapiWrapper mapiWrapper, ITwainController twainController)
        : this(scanningContext, new RemoteScanController(scanningContext),
            thumbnailRenderer, mapiWrapper, twainController)
    {
    }

    internal WorkerServiceImpl(ScanningContext scanningContext, IRemoteScanController remoteScanController,
        ThumbnailRenderer thumbnailRenderer,
        IMapiWrapper mapiWrapper, ITwainController twainController)
    {
        _scanningContext = scanningContext;
        _remoteScanController = remoteScanController;
        _thumbnailRenderer = thumbnailRenderer;
        _mapiWrapper = mapiWrapper;
        _twainController = twainController;
        _scanningContext = scanningContext;
    }

    public override Task<InitResponse> Init(InitRequest request, ServerCallContext context)
    {
        using var callRef = StartCall();
        try
        {
            if (!string.IsNullOrEmpty(request.RecoveryFolderPath))
            {
                // TODO: Rename "Recovery" folder path to something more generic
                _scanningContext.FileStorageManager = new FileStorageManager(request.RecoveryFolderPath);
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
        using var callRef = StartCall();
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

    public override async Task<GetDeviceListResponse> GetDeviceList(GetDeviceListRequest request,
        ServerCallContext context)
    {
        using var callRef = StartCall();
        try
        {
            var scanOptions = request.OptionsXml.FromXml<ScanOptions>();
            var deviceList = await _remoteScanController.GetDeviceList(scanOptions);
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

    public override async Task Scan(ScanRequest request, IServerStreamWriter<ScanResponse> responseStream,
        ServerCallContext context)
    {
        using var callRef = StartCall();
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
            await _remoteScanController.Scan(request.OptionsXml.FromXml<ScanOptions>(),
                context.CancellationToken, scanEvents,
                (image, postProcessingContext) =>
                    sequencedWriter.Write(new ScanResponse
                    {
                        Image = SerializedImageHelper.Serialize(image,
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

    public override async Task<SendMapiEmailResponse> SendMapiEmail(SendMapiEmailRequest request,
        ServerCallContext context)
    {
        using var callRef = StartCall();
        try
        {
            var emailMessage = request.EmailMessageXml.FromXml<EmailMessage>();
            var returnCode = await _mapiWrapper.SendEmail(request.ClientName, emailMessage);
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

    public override async Task<RenderThumbnailResponse> RenderThumbnail(RenderThumbnailRequest request,
        ServerCallContext context)
    {
        using var callRef = StartCall();
        try
        {
            var deserializeOptions = new SerializedImageHelper.DeserializeOptions
            {
                ShareFileStorage = true
            };
            using var image =
                SerializedImageHelper.Deserialize(_scanningContext, request.Image, deserializeOptions);
            var thumbnail = _thumbnailRenderer.Render(image, request.Size);
            var stream = thumbnail.SaveToMemoryStream(ImageFileFormat.Png);
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
        using var callRef = StartCall();
        try
        {
            var renderer = new PdfiumPdfRenderer(_scanningContext.ImageContext);
            using var image = renderer.Render(request.Path, request.Dpi).Single();
            var stream = image.SaveToMemoryStream(ImageFileFormat.Png);
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

    public override Task<StopWorkerResponse> StopWorker(StopWorkerRequest request, ServerCallContext context)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                lock (this)
                {
                    if (_ongoingCallCount <= 0)
                    {
                        OnStop?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                }
                await _ongoingCallFinished.WaitOneAsync();
            }
        }).AssertNoAwait();
        return Task.FromResult(new StopWorkerResponse());
    }

    public override async Task TwainScan(TwainScanRequest request,
        IServerStreamWriter<TwainScanResponse> responseStream, ServerCallContext context)
    {
        using var callRef = StartCall();
        var sequencedWriter = new SequencedWriter<TwainScanResponse>(responseStream);
        try
        {
            var twainEvents = new TwainEvents(
                pageStart => sequencedWriter.Write(new TwainScanResponse
                {
                    PageStart = pageStart
                }),
                nativeImage => sequencedWriter.Write(new TwainScanResponse
                {
                    NativeImage = nativeImage
                }),
                memoryBuffer => sequencedWriter.Write(new TwainScanResponse
                {
                    MemoryBuffer = memoryBuffer
                })
            );
            var options = request.OptionsXml.FromXml<ScanOptions>();
            await _twainController.StartScan(options, twainEvents, context.CancellationToken);
        }
        catch (Exception e)
        {
            sequencedWriter.Write(new TwainScanResponse { Error = RemotingHelper.ToError(e) });
        }
        await sequencedWriter.WaitForCompletion();
    }

    public event EventHandler? OnStop;

    private CallReference StartCall() => new(this);

    private class CallReference : IDisposable
    {
        private readonly WorkerServiceImpl _owner;

        public CallReference(WorkerServiceImpl owner)
        {
            _owner = owner;
            lock (owner)
            {
                owner._ongoingCallCount++;
            }
        }

        public void Dispose()
        {
            lock (_owner)
            {
                _owner._ongoingCallCount--;
                _owner._ongoingCallFinished.Set();
            }
        }
    }
}