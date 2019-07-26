using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.Scan.Twain;
using NAPS2.Scan.Wia;
using NAPS2.Scan.Wia.Native;
using NAPS2.Util;

namespace NAPS2.Worker
{
    /// <summary>
    /// The WCF service implementation for NAPS2.Worker.exe.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession,
        IncludeExceptionDetailInFaults = true,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WorkerService : IWorkerService
    {
        private readonly TwainWrapper twainWrapper;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly MapiWrapper mapiWrapper;

        private CancellationTokenSource twainScanCts = new CancellationTokenSource();

        public WorkerService(TwainWrapper twainWrapper, ThumbnailRenderer thumbnailRenderer, MapiWrapper mapiWrapper)
        {
            this.twainWrapper = twainWrapper;
            this.thumbnailRenderer = thumbnailRenderer;
            this.mapiWrapper = mapiWrapper;
        }

        public void Init(string recoveryFolderPath)
        {
            Callback = OperationContext.Current.GetCallbackChannel<IWorkerCallback>();
            RecoveryImage.RecoveryFolder = new DirectoryInfo(recoveryFolderPath);
        }

        public WiaConfiguration Wia10NativeUI(string deviceId, IntPtr hwnd)
        {
            try
            {
                try
                {
                    using (var deviceManager = new WiaDeviceManager(WiaVersion.Wia10))
                    using (var device = deviceManager.FindDevice(deviceId))
                    {
                        var item = device.PromptToConfigure(hwnd);
                        return new WiaConfiguration
                        {
                            DeviceProps = device.Properties.SerializeEditable(),
                            ItemProps = item.Properties.SerializeEditable(),
                            ItemName = item.Name()
                        };
                    }
                }
                catch (WiaException e)
                {
                    WiaScanErrors.ThrowDeviceError(e);
                    throw new InvalidOperationException();
                }
            }
            catch (ScanDriverException e)
            {
                throw new FaultException<ScanDriverExceptionDetail>(new ScanDriverExceptionDetail(e));
            }
        }

        public List<ScanDevice> TwainGetDeviceList(TwainImpl twainImpl)
        {
            return twainWrapper.GetDeviceList(twainImpl);
        }

        public async Task TwainScan(ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, IntPtr hwnd)
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    var imagePathDict = new Dictionary<ScannedImage, string>();
                    twainWrapper.Scan(hwnd == IntPtr.Zero ? null : new Win32Window(hwnd), scanDevice, scanProfile, scanParams, twainScanCts.Token,
                        new WorkerImageSource(Callback, imagePathDict), (img, _, path) => imagePathDict.Add(img, path));
                }, TaskCreationOptions.LongRunning);
            }
            catch (ScanDriverException e)
            {
                throw new FaultException<ScanDriverExceptionDetail>(new ScanDriverExceptionDetail(e));
            }
        }

        public void CancelTwainScan()
        {
            twainScanCts.Cancel();
        }

        public MapiSendMailReturnCode SendMapiEmail(string clientName, EmailMessage message)
        {
            return mapiWrapper.SendEmail(clientName, message);
        }

        public byte[] RenderThumbnail(ScannedImage.Snapshot snapshot, int size)
        {
            var stream = new MemoryStream();
            using (var bitmap = Task.Factory.StartNew(() => thumbnailRenderer.RenderThumbnail(snapshot, size)).Unwrap().Result)
            {
                bitmap.Save(stream, ImageFormat.Png);
            }
            return stream.ToArray();
        }

        public void Dispose()
        {
        }

        public IWorkerCallback Callback { get; set; }

        private class WorkerImageSource : ScannedImageSource.Concrete
        {
            private readonly IWorkerCallback callback;
            private readonly Dictionary<ScannedImage, string> imagePathDict;

            public WorkerImageSource(IWorkerCallback callback, Dictionary<ScannedImage, string> imagePathDict)
            {
                this.callback = callback;
                this.imagePathDict = imagePathDict;
            }

            public override void Put(ScannedImage image)
            {
                MemoryStream stream = null;
                var thumb = image.GetThumbnail();
                if (thumb != null)
                {
                    stream = new MemoryStream();
                    thumb.Save(stream, ImageFormat.Png);
                }
                callback.TwainImageReceived(image.RecoveryIndexImage, stream?.ToArray(), imagePathDict.Get(image));
            }
        }
    }
}
