using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.WinForms;

namespace NAPS2.ClientServer
{
    public class ProxiedScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "proxy";

        private readonly ClientContextFactory clientContextFactory;

        public ProxiedScanDriver(ClientContextFactory clientContextFactory, IFormFactory formFactory)
            : base(formFactory)
        {
            this.clientContextFactory = clientContextFactory;
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
                return client.Service.GetDeviceList(ScanProfile.ProxyConfig.RemoteDriverName);
            }
        }

        protected override Task ScanInternal(ScannedImageSource.Concrete source)
        {
            if (ScanProfile.ProxyConfig == null)
            {
                throw new InvalidOperationException("ScanProfile.ProxyConfig must be specified to use ProxiedScanDriver.");
            }

            return Task.Factory.StartNew(() =>
            {
                using (var client = clientContextFactory.Create(ScanProfile.ProxyConfig))
                {
                    client.Callback.ImageCallback += (imageBytes, indexImage) =>
                    {
                        indexImage.FileName = RecoveryImage.GetNextFileName() + Path.GetExtension(indexImage.FileName);
                        var recoveryFilePath = Path.Combine(RecoveryImage.RecoveryFolder.FullName, indexImage.FileName);
                        File.WriteAllBytes(recoveryFilePath, imageBytes);
                        var image = new ScannedImage(indexImage);
                        // TODO: Post-processing etc.
                        // TODO: Also add a ScanParams flag to disable post-processing on the server
                        source.Put(image);
                    };
                    client.Service.Scan(ScanProfile, ScanParams);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
