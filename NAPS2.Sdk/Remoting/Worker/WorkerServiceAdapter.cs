using System.Threading;
using Grpc.Core;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Scan.Internal.Twain;
using NAPS2.Scan.Wia;
using NAPS2.Serialization;

namespace NAPS2.Remoting.Worker;

public class WorkerServiceAdapter
{
    private readonly WorkerService.WorkerServiceClient _client;

    public WorkerServiceAdapter(CallInvoker callInvoker)
    {
        _client = new WorkerService.WorkerServiceClient(callInvoker);
    }

    public void Init(string recoveryFolderPath)
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

    public async Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        var req = new GetDeviceListRequest { OptionsXml = options.ToXml() };
        var resp = await _client.GetDeviceListAsync(req);
        RemotingHelper.HandleErrors(resp.Error);
        return resp.DeviceListXml.FromXml<List<ScanDevice>>();
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
                    var renderableImage = SerializedImageHelper.Deserialize(scanningContext, resp.Image,
                        new SerializedImageHelper.DeserializeOptions());
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
            Image = SerializedImageHelper.Serialize(image, new SerializedImageHelper.SerializeOptions
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
        _client.StopWorkerAsync(new StopWorkerRequest());
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
}