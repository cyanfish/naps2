using System.Threading;
using Grpc.Core;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
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

    public WiaConfiguration Wia10NativeUI(string scanDevice, IntPtr hwnd)
    {
        var req = new Wia10NativeUiRequest
        {
            DeviceId = scanDevice,
            Hwnd = (ulong)hwnd
        };
        var resp = _client.Wia10NativeUi(req);
        RemotingHelper.HandleErrors(resp.Error);
        return resp.WiaConfigurationXml.FromXml<WiaConfiguration>();
    }

    public async Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        var req = new GetDeviceListRequest { OptionsXml = options.ToXml() };
        var resp = await _client.GetDeviceListAsync(req);
        RemotingHelper.HandleErrors(resp.Error);
        return resp.DeviceListXml.FromXml<List<ScanDevice>>();
    }

    public async Task Scan(ScanningContext scanningContext, ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<RenderableImage, string> imageCallback)
    {
        var req = new ScanRequest
        {
            OptionsXml = options.ToXml()
        };
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
                var renderableImage = SerializedImageHelper.Deserialize(scanningContext, resp.Image, new SerializedImageHelper.DeserializeOptions());
                imageCallback?.Invoke(renderableImage, resp.Image.RenderedFilePath);
            }
        }
    }

    public async Task<MapiSendMailReturnCode> SendMapiEmail(EmailMessage message)
    {
        var req = new SendMapiEmailRequest { EmailMessageXml = message.ToXml() };
        var resp = await _client.SendMapiEmailAsync(req);
        RemotingHelper.HandleErrors(resp.Error);
        return resp.ReturnCodeXml.FromXml<MapiSendMailReturnCode>();
    }

    public byte[] RenderThumbnail(ImageContext imageContext, RenderableImage image, int size)
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
}