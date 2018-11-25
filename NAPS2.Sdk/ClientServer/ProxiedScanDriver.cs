using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Logging;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.ClientServer
{
    public class ProxiedScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "proxy";

        private readonly ClientContextFactory clientContextFactory;
        private readonly ScannedImageHelper scannedImageHelper;
        private readonly IFormFactory formFactory;

        public ProxiedScanDriver(ClientContextFactory clientContextFactory, IFormFactory formFactory, ScannedImageHelper scannedImageHelper)
            : base(formFactory)
        {
            this.clientContextFactory = clientContextFactory;
            this.formFactory = formFactory;
            this.scannedImageHelper = scannedImageHelper;
        }

        public override string DriverName => DRIVER_NAME;

        public override bool IsSupported => true;

        protected override List<ScanDevice> GetDeviceListInternal()
        {
            if (ScanProfile == null)
            {
                throw new InvalidOperationException("ScanProfile must be set before calling methods on ProxiedScanDriver.");
            }
            if (ScanProfile.ProxyConfig == null)
            {
                throw new InvalidOperationException("ScanProfile.ProxyConfig must be specified to use ProxiedScanDriver.");
            }

            using (var client = clientContextFactory.Create(ScanProfile.ProxyConfig))
            {
                return client.Service.GetDeviceList(ScanProfile);
            }
        }

        protected override Task ScanInternal(ScannedImageSource.Concrete source)
        {
            if (ScanProfile.ProxyConfig == null)
            {
                throw new InvalidOperationException("ScanProfile.ProxyConfig must be specified to use ProxiedScanDriver.");
            }

            return Task.Factory.StartNew(async () =>
            {
                try
                {
                    using (var client = clientContextFactory.Create(ScanProfile.ProxyConfig))
                    {
                        var noUi = ScanParams.NoUI;
                        FScanProgress form = Invoker.Current.InvokeGet(() => noUi ? null : formFactory.Create<FScanProgress>());
                        int pageNumber = 1;
                        var sem = new Semaphore(0, int.MaxValue);

                        client.Callback.ImageCallback += (imageBytes, indexImage) =>
                        {
                            try
                            {
                                indexImage.FileName = RecoveryImage.GetNextFileName() + Path.GetExtension(indexImage.FileName);
                                var recoveryFilePath = Path.Combine(RecoveryImage.RecoveryFolder.FullName, indexImage.FileName);
                                File.WriteAllBytes(recoveryFilePath, imageBytes);
                                var image = new ScannedImage(indexImage);
                                using (var bitmap = new Bitmap(new MemoryStream(imageBytes)))
                                {
                                    scannedImageHelper.PostProcessStep2(image, bitmap, ScanProfile, ScanParams, pageNumber++, false);
                                }

                                source.Put(image);
                                if (form != null)
                                {
                                    form.PageNumber = pageNumber;
                                    Invoker.Current.SafeInvoke(() => form.RefreshStatus());
                                }
                            }
                            finally
                            {
                                sem.Release();
                            }
                        };

                        var scanTask = client.Service.Scan(ScanProfile, ScanParams).ContinueWith(t =>
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
                        CancelToken.Register(client.Service.CancelScan);

                        if (noUi)
                        {
                            await scanTask;
                        }
                        else if (ScanParams.Modal)
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
