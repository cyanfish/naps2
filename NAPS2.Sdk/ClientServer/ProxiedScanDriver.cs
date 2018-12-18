using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Logging;
using NAPS2.Scan;
using NAPS2.Images;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.ClientServer
{
    public class ProxiedScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "proxy";

        private readonly ClientContextFactory clientContextFactory;

        public ProxiedScanDriver(ClientContextFactory clientContextFactory)
        {
            this.clientContextFactory = clientContextFactory;
        }

        public override string DriverName => DRIVER_NAME;

        public override bool IsSupported => true;

        protected override List<ScanDevice> GetDeviceListInternal(ScanProfile scanProfile)
        {
            if (scanProfile?.ProxyConfig == null)
            {
                throw new ArgumentException("ScanProfile.ProxyConfig must be specified to use ProxiedScanDriver.", nameof(scanProfile));
            }

            using (var client = clientContextFactory.Create(scanProfile.ProxyConfig))
            {
                return client.Service.GetDeviceList(scanProfile);
            }
        }

        protected override Task ScanInternal(ScannedImageSource.Concrete source, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent, CancellationToken cancelToken)
        {
            if (scanProfile.ProxyConfig == null)
            {
                throw new InvalidOperationException("ScanProfile.ProxyConfig must be specified to use ProxiedScanDriver.");
            }

            return Task.Factory.StartNew(async () =>
            {
                try
                {
                    using (var client = clientContextFactory.Create(scanProfile.ProxyConfig))
                    {
                        var noUi = scanParams.NoUI;
                        FScanProgress form = Invoker.Current.InvokeGet(() => noUi ? null : new FScanProgress());
                        int pageNumber = 1;
                        var sem = new Semaphore(0, int.MaxValue);

                        client.Callback.ImageCallback += (imageBytes, indexImage) =>
                        {
                            try
                            {
                                // TODO
                                //indexImage.FileName = RecoveryImage.GetNextFileName() + Path.GetExtension(indexImage.FileName);
                                //var recoveryFilePath = Path.Combine(RecoveryImage.RecoveryFolder.FullName, indexImage.FileName);
                                //File.WriteAllBytes(recoveryFilePath, imageBytes);
                                //var image = new ScannedImage(indexImage);
                                //using (var bitmap = new Bitmap(new MemoryStream(imageBytes)))
                                //{
                                //    scannedImageHelper.PostProcessStep2(image, bitmap, ScanProfile, ScanParams, pageNumber++, false);
                                //}

                                //source.Put(image);
                                //if (form != null)
                                //{
                                //    form.PageNumber = pageNumber;
                                //    Invoker.Current.SafeInvoke(() => form.RefreshStatus());
                                //}
                            }
                            finally
                            {
                                sem.Release();
                            }
                        };

                        var scanTask = client.Service.Scan(scanProfile, scanParams).ContinueWith(t =>
                        {
                            for (int i = 0; i < t.Result; i++)
                            {
                                sem.WaitOne();
                            }
                        });

                        if (!noUi)
                        {
                            form.PageNumber = pageNumber;
                            form.AsyncTransfer = async () => await scanTask;
                            form.CancelToken.Register(client.Service.CancelScan);
                        }
                        cancelToken.Register(client.Service.CancelScan);

                        if (noUi)
                        {
                            await scanTask;
                        }
                        else if (scanParams.Modal)
                        {
                            Invoker.Current.SafeInvoke(() => form.ShowDialog());
                        }
                        else
                        {
                            Invoker.Current.SafeInvoke(() => form.Show());
                            await scanTask;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.ErrorException("Error scanning with proxy", e);
                }
            }, TaskCreationOptions.LongRunning).Unwrap();
        }
    }
}
