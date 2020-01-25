using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Scan.Wia;
using NAPS2.Serialization;

namespace NAPS2.Remoting.Worker
{
    public class WorkerServiceAdapter
    {
        private readonly WorkerService.WorkerServiceClient client;

        public WorkerServiceAdapter(CallInvoker callInvoker)
        {
            client = new WorkerService.WorkerServiceClient(callInvoker);
        }

        public void Init(string recoveryFolderPath)
        {
            var req = new InitRequest { RecoveryFolderPath = recoveryFolderPath ?? "" };
            var resp = client.Init(req);
            RemotingHelper.HandleErrors(resp.Error);
        }

        public WiaConfiguration Wia10NativeUI(string scanDevice, IntPtr hwnd)
        {
            var req = new Wia10NativeUiRequest
            {
                DeviceId = scanDevice,
                Hwnd = (ulong)hwnd
            };
            var resp = client.Wia10NativeUi(req);
            RemotingHelper.HandleErrors(resp.Error);
            return resp.WiaConfigurationXml.FromXml<WiaConfiguration>();
        }

        public async Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
        {
            var req = new GetDeviceListRequest { OptionsXml = options.ToXml() };
            var resp = await client.GetDeviceListAsync(req);
            RemotingHelper.HandleErrors(resp.Error);
            return resp.DeviceListXml.FromXml<List<ScanDevice>>();
        }

        public async Task Scan(ImageContext imageContext, ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<ScannedImage, string> imageCallback)
        {
            var req = new ScanRequest
            {
                OptionsXml = options.ToXml()
            };
            var streamingCall = client.Scan(req, cancellationToken: cancelToken);
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
                    var scannedImage = SerializedImageHelper.Deserialize(imageContext, resp.Image, new SerializedImageHelper.DeserializeOptions());
                    imageCallback?.Invoke(scannedImage, resp.Image.RenderedFilePath);
                }
            }
        }

        public MapiSendMailReturnCode SendMapiEmail(EmailMessage message)
        {
            var req = new SendMapiEmailRequest { EmailMessageXml = message.ToXml() };
            var resp = client.SendMapiEmail(req);
            RemotingHelper.HandleErrors(resp.Error);
            return resp.ReturnCodeXml.FromXml<MapiSendMailReturnCode>();
        }

        public byte[] RenderThumbnail(ImageContext imageContext, ScannedImage.Snapshot snapshot, int size)
        {
            var req = new RenderThumbnailRequest
            {
                Image = SerializedImageHelper.Serialize(imageContext, snapshot, new SerializedImageHelper.SerializeOptions
                {
                    RequireFileStorage = true
                }),
                Size = size
            };
            var resp = client.RenderThumbnail(req);
            RemotingHelper.HandleErrors(resp.Error);
            return resp.Thumbnail.ToByteArray();
        }
    }
}