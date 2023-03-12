using System.Threading;
using Google.Protobuf;
using Grpc.Core;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.ImportExport.Images;
using NAPS2.Pdf;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Scan.Internal.Twain;
#if !MAC
using NAPS2.Scan.Internal.Wia;
using NAPS2.Wia;
#endif
using NAPS2.Serialization;

namespace NAPS2.Remoting.Worker;

public class WorkerServiceImpl : WorkerService.WorkerServiceBase
{
    private readonly ScanningContext _scanningContext;
    private readonly IRemoteScanController _remoteScanController;
    private readonly ThumbnailRenderer _thumbnailRenderer;
    private readonly IMapiWrapper _mapiWrapper;
    private readonly ITwainSessionController _twainSessionController;
    private readonly ImportPostProcessor _importPostProcessor;

    private readonly AutoResetEvent _ongoingCallFinished = new(false);
    private int _ongoingCallCount;

    public WorkerServiceImpl(ScanningContext scanningContext, ThumbnailRenderer thumbnailRenderer,
        IMapiWrapper mapiWrapper, ITwainSessionController twainSessionController)
        : this(scanningContext, new RemoteScanController(scanningContext),
            thumbnailRenderer, mapiWrapper, twainSessionController,
            new ImportPostProcessor())
    {
    }

    internal WorkerServiceImpl(ScanningContext scanningContext, IRemoteScanController remoteScanController,
        ThumbnailRenderer thumbnailRenderer,
        IMapiWrapper mapiWrapper, ITwainSessionController twainSessionController,
        ImportPostProcessor importPostProcessor)
    {
        _scanningContext = scanningContext;
        _remoteScanController = remoteScanController;
        _thumbnailRenderer = thumbnailRenderer;
        _mapiWrapper = mapiWrapper;
        _twainSessionController = twainSessionController;
        _importPostProcessor = importPostProcessor;
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
#if MAC
        throw new NotSupportedException();
#else
        using var callRef = StartCall();
        try
        {
            try
            {
                using var deviceManager = new WiaDeviceManager(Wia.WiaVersion.Wia10);
                using var device = deviceManager.FindDevice(request.DeviceId);
                var item = device.PromptToConfigure((IntPtr) request.Hwnd);
                var response = new Wia10NativeUiResponse();
                if (item != null)
                {
                    response.WiaConfigurationXml = new WiaConfiguration
                    {
                        DeviceProps = device.Properties.SerializeEditable(),
                        ItemProps = item.Properties.SerializeEditable(),
                        ItemName = item.Name()
                    }.ToXml();
                }
                return Task.FromResult(response);
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
#endif
    }

    public override async Task GetDevices(GetDevicesRequest request,
        IServerStreamWriter<GetDevicesResponse> responseStream, ServerCallContext context)
    {
        using var callRef = StartCall();
        var sequencedWriter = new SequencedWriter<GetDevicesResponse>(responseStream);
        try
        {
            var scanOptions = request.OptionsXml.FromXml<ScanOptions>();
            await _remoteScanController.GetDevices(scanOptions,
                context.CancellationToken,
                device => sequencedWriter.Write(new GetDevicesResponse
                {
                    DeviceXml = device.ToXml()
                }));
        }
        catch (Exception e)
        {
            sequencedWriter.Write(new GetDevicesResponse { Error = RemotingHelper.ToError(e) });
        }
        await sequencedWriter.WaitForCompletion();
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
                        Image = ImageSerializer.Serialize(image,
                            new SerializeImageOptions
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

    public override Task<LoadMapiResponse> LoadMapi(LoadMapiRequest request,
        ServerCallContext context)
    {
        using var callRef = StartCall();
        try
        {
            var loaded = _mapiWrapper.CanLoadClient(request.ClientName);
            return Task.FromResult(new LoadMapiResponse
            {
                Loaded = loaded
            });
        }
        catch (Exception)
        {
            return Task.FromResult(new LoadMapiResponse
            {
                Loaded = false
            });
        }
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

    public override Task<RenderThumbnailResponse> RenderThumbnail(RenderThumbnailRequest request,
        ServerCallContext context)
    {
        using var callRef = StartCall();
        try
        {
            var deserializeOptions = new DeserializeImageOptions
            {
                ShareFileStorage = true
            };
            using var image =
                ImageSerializer.Deserialize(_scanningContext, request.Image, deserializeOptions);
            var thumbnail = _thumbnailRenderer.Render(image, request.Size);
            var stream = thumbnail.SaveToMemoryStream(ImageFileFormat.Png);
            return Task.FromResult(new RenderThumbnailResponse
            {
                Thumbnail = ByteString.FromStream(stream)
            });
        }
        catch (Exception e)
        {
            return Task.FromResult(new RenderThumbnailResponse { Error = RemotingHelper.ToError(e) });
        }
    }

    public override Task<RenderPdfResponse> RenderPdf(RenderPdfRequest request, ServerCallContext context)
    {
        using var callRef = StartCall();
        try
        {
            var renderer = new PdfiumPdfRenderer();
            using var image = renderer
                .Render(_scanningContext.ImageContext, request.Path, PdfRenderSize.FromDpi(request.Dpi)).Single();
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

    public override Task<ImportPostProcessResponse> ImportPostProcess(ImportPostProcessRequest request,
        ServerCallContext context)
    {
        using var callRef = StartCall();
        try
        {
            using var image =
                ImageSerializer.Deserialize(_scanningContext, request.Image, new DeserializeImageOptions());
            int? thumbnailSize = request.ThumbnailSize == 0 ? null : request.ThumbnailSize;
            var barcodeOptions = request.BarcodeDetectionOptionsXml.FromXml<BarcodeDetectionOptions>();
            using var newImage =
                _importPostProcessor.AddPostProcessingData(image, null, thumbnailSize, barcodeOptions, true);
            return Task.FromResult(new ImportPostProcessResponse
            {
                Image = ImageSerializer.Serialize(newImage,
                    new SerializeImageOptions
                    {
                        RequireFileStorage = true,
                        ReturnOwnership = true,
                        IncludeThumbnail = true
                    })
            });
        }
        catch (Exception e)
        {
            return Task.FromResult(new ImportPostProcessResponse { Error = RemotingHelper.ToError(e) });
        }
    }

    public override Task<StopWorkerResponse> StopWorker(StopWorkerRequest request, ServerCallContext context)
    {
        Stop();
        return Task.FromResult(new StopWorkerResponse());
    }

    public void Stop()
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
    }

    public override async Task<GetDeviceListResponse> TwainGetDeviceList(GetDeviceListRequest request,
        ServerCallContext context)
    {
        using var callRef = StartCall();
        try
        {
            var options = request.OptionsXml.FromXml<ScanOptions>();
            var deviceList = await _twainSessionController.GetDeviceList(options);
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
                }),
                canceled => sequencedWriter.Write(new TwainScanResponse
                {
                    TransferCanceled = canceled
                })
            );
            var options = request.OptionsXml.FromXml<ScanOptions>();
            await _twainSessionController.StartScan(options, twainEvents, context.CancellationToken);
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