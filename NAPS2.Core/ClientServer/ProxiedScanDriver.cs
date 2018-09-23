using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        public ProxiedScanDriver(ClientContextFactory clientContextFactory, IFormFactory formFactory, ScannedImageHelper scannedImageHelper)
            : base(formFactory)
        {
            this.clientContextFactory = clientContextFactory;
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
                        int pageNumber = 1;
                        client.Callback.ImageCallback += (imageBytes, indexImage) =>
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
                        };
                        await client.Service.Scan(ScanProfile, ScanParams);
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
