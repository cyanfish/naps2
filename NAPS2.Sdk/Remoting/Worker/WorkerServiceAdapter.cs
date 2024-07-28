using System.Threading;
using Grpc.Core;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Scan.Internal.Twain;
using NAPS2.Scan.Internal.Wia;
using NAPS2.Serialization;

namespace NAPS2.Remoting.Worker;

internal class WorkerServiceAdapter
{
    private readonly WorkerService.WorkerServiceClient _client;

    public WorkerServiceAdapter(CallInvoker callInvoker)
    {
        _client = new WorkerService.WorkerServiceClient(callInvoker);
    }

    public void Init(string? recoveryFolderPath)
    {
        var req = new InitRequest { RecoveryFolderPath = recoveryFolderPath ?? "" };
        var resp = _client.Init(req);
        RemotingHelper.HandleErrors(resp.Error);
    }

    public WiaConfiguration? Wia10NativeUI(string scanDevice, IntPtr hwnd)
    {
        var req = new Wia10NativeUiRequest
        {
            DeviceId = scanDevice,
            Hwnd = (ulong) hwnd
        };
        var resp = _client.Wia10NativeUi(req);
        RemotingHelper.HandleErrors(resp.Error);
        if (string.IsNullOrEmpty(resp.WiaConfigurationXml))
        {
            return null;
        }
        return resp.WiaConfigurationXml.FromXml<WiaConfiguration>();
    }

    public async Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback)
    {
        var req = new GetDevicesRequest
        {
            OptionsXml = options.ToXml()
        };
        try
        {
            var streamingCall = _client.GetDevices(req, cancellationToken: cancelToken);
            while (await streamingCall.ResponseStream.MoveNext())
            {
                var resp = streamingCall.ResponseStream.Current;
                RemotingHelper.HandleErrors(resp.Error);
                callback(resp.DeviceXml.FromXml<ScanDevice>());
            }
        }
        catch (RpcException ex)
        {
            if (ex.Status.StatusCode != StatusCode.Cancelled)
            {
                throw;
            }
        }
    }

    public async Task<ScanCaps?> GetCaps(ScanOptions options, CancellationToken cancelToken)
    {
        var req = new GetCapsRequest { OptionsXml = options.ToXml() };
        var resp = await _client.GetCapsAsync(req, cancellationToken: cancelToken);
        RemotingHelper.HandleErrors(resp.Error);
        return string.IsNullOrEmpty(resp.ScanCapsXml) ? null : resp.ScanCapsXml.FromXml<ScanCaps>();
    }

    public async Task Scan(ScanningContext scanningContext, ScanOptions options, CancellationToken cancelToken,
        IScanEvents scanEvents, Action<ProcessedImage, string> imageCallback)
    {
        var req = new ScanRequest
        {
            OptionsXml = options.ToXml()
        };
        try
        {
            var streamingCall = _client.Scan(req, cancellationToken: cancelToken);
            while (await streamingCall.ResponseStream.MoveNext())
            {
                var resp = streamingCall.ResponseStream.Current;
                RemotingHelper.HandleErrors(resp.Error);
                if (resp.PageStart != null)
                {
                    scanEvents.PageStart();
                }
                if (resp.Progress != null)
                {
                    scanEvents.PageProgress(resp.Progress.Value);
                }
                if (resp.Image != null)
                {
                    var renderableImage = ImageSerializer.Deserialize(scanningContext, resp.Image,
                        new DeserializeImageOptions());
                    imageCallback?.Invoke(renderableImage, resp.Image.RenderedFilePath);
                }
            }
        }
        catch (RpcException ex)
        {
            if (ex.Status.StatusCode != StatusCode.Cancelled)
            {
                throw;
            }
        }
    }

    public async Task<bool> CanLoadMapi(string? clientName)
    {
        var req = new LoadMapiRequest { ClientName = clientName };
        var resp = await _client.LoadMapiAsync(req);
        return resp.Loaded;
    }

    public async Task<MapiSendMailReturnCode> SendMapiEmail(string? clientName, EmailMessage message)
    {
        var req = new SendMapiEmailRequest { ClientName = clientName, EmailMessageXml = message.ToXml() };
        var resp = await _client.SendMapiEmailAsync(req);
        RemotingHelper.HandleErrors(resp.Error);
        return resp.ReturnCodeXml.FromXml<MapiSendMailReturnCode>();
    }

    public byte[] RenderThumbnail(ImageContext imageContext, ProcessedImage image, int size)
    {
        var req = new RenderThumbnailRequest
        {
            Image = ImageSerializer.Serialize(image, new SerializeImageOptions
            {
                RequireFileStorage = true
            }),
            Size = size
        };
        var resp = _client.RenderThumbnail(req);
        RemotingHelper.HandleErrors(resp.Error);
        return resp.Thumbnail.ToByteArray();
    }

    public byte[] RenderPdf(string path, float dpi)
    {
        var req = new RenderPdfRequest
        {
            Path = path,
            Dpi = dpi
        };
        var resp = _client.RenderPdf(req);
        RemotingHelper.HandleErrors(resp.Error);
        return resp.Image.ToByteArray();
    }

    public void StopWorker()
    {
        _client.StopWorker(new StopWorkerRequest());
    }

    public async Task TwainScan(ScanOptions options, CancellationToken cancelToken, ITwainEvents twainEvents)
    {
        var req = new TwainScanRequest
        {
            OptionsXml = options.ToXml()
        };
        try
        {
            var streamingCall = _client.TwainScan(req, cancellationToken: cancelToken);
            while (await streamingCall.ResponseStream.MoveNext())
            {
                var resp = streamingCall.ResponseStream.Current;
                RemotingHelper.HandleErrors(resp.Error);
                if (resp.PageStart != null)
                {
                    twainEvents.PageStart(resp.PageStart);
                }
                if (resp.NativeImage != null)
                {
                    twainEvents.NativeImageTransferred(resp.NativeImage);
                }
                if (resp.MemoryBuffer != null)
                {
                    twainEvents.MemoryBufferTransferred(resp.MemoryBuffer);
                }
                if (resp.TransferCanceled != null)
                {
                    twainEvents.TransferCanceled(resp.TransferCanceled);
                }
            }
        }
        catch (RpcException ex)
        {
            if (ex.Status.StatusCode != StatusCode.Cancelled)
            {
                throw;
            }
        }
    }

    public async Task<List<ScanDevice>> TwainGetDeviceList(ScanOptions options)
    {
        var req = new GetDeviceListRequest { OptionsXml = options.ToXml() };
        var resp = await _client.TwainGetDeviceListAsync(req);
        RemotingHelper.HandleErrors(resp.Error);
        return resp.DeviceListXml.FromXml<List<ScanDevice>>();
    }

    public async Task<ScanCaps?> TwainGetCaps(ScanOptions options)
    {
        var req = new GetCapsRequest { OptionsXml = options.ToXml() };
        var resp = await _client.TwainGetCapsAsync(req);
        RemotingHelper.HandleErrors(resp.Error);
        return string.IsNullOrEmpty(resp.ScanCapsXml) ? null : resp.ScanCapsXml.FromXml<ScanCaps>();
    }

    public ProcessedImage ImportPostProcess(ScanningContext scanningContext, ProcessedImage img, int? thumbnailSize,
        BarcodeDetectionOptions barcodeDetectionOptions)
    {
        var req = new ImportPostProcessRequest
        {
            Image = ImageSerializer.Serialize(img, new SerializeImageOptions
            {
                RequireFileStorage = true,
                TransferOwnership = true
            }),
            ThumbnailSize = thumbnailSize ?? 0,
            BarcodeDetectionOptionsXml = barcodeDetectionOptions.ToXml()
        };
        var resp = _client.ImportPostProcess(req);
        RemotingHelper.HandleErrors(resp.Error);
        return ImageSerializer.Deserialize(scanningContext, resp.Image, new DeserializeImageOptions());
    }
}