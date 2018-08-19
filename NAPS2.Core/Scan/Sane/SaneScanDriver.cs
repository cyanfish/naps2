using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.WinForms;

namespace NAPS2.Scan.Sane
{
    public class SaneScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "sane";

        private readonly SaneWrapper saneWrapper;
        private readonly IFormFactory formFactory;
        private readonly IBlankDetector blankDetector;
        private readonly ThumbnailRenderer thumbnailRenderer;
        private readonly ScannedImageHelper scannedImageHelper;

        public SaneScanDriver(SaneWrapper saneWrapper, IFormFactory formFactory, IBlankDetector blankDetector, ThumbnailRenderer thumbnailRenderer, ScannedImageHelper scannedImageHelper)
        {
            this.saneWrapper = saneWrapper;
            this.formFactory = formFactory;
            this.blankDetector = blankDetector;
            this.thumbnailRenderer = thumbnailRenderer;
            this.scannedImageHelper = scannedImageHelper;
        }

        public override string DriverName => DRIVER_NAME;

        protected override ScanDevice PromptForDeviceInternal()
        {
            var deviceList = GetDeviceList();

            if (!deviceList.Any())
            {
                throw new NoDevicesFoundException();
            }

            var form = formFactory.Create<FSelectDevice>();
            form.DeviceList = deviceList;
            form.ShowDialog();
            return form.SelectedDevice;
        }

        protected override List<ScanDevice> GetDeviceListInternal()
        {
            return saneWrapper.GetDeviceList().ToList();
        }

        protected override IEnumerable<ScannedImage> ScanInternal()
        {
            // TODO: Support ADF
            using (Bitmap output = saneWrapper.ScanOne(ScanDevice.ID, ScanProfile.KeyValueOptions))
            {
                using (var result = scannedImageHelper.PostProcessStep1(output, ScanProfile))
                {
                    if (blankDetector.ExcludePage(result, ScanProfile))
                    {
                        yield break;
                    }
                    // TODO: Set bit depth correctly
                    var image = new ScannedImage(result, ScanProfile.BitDepth, ScanProfile.MaxQuality, ScanProfile.Quality);
                    image.SetThumbnail(thumbnailRenderer.RenderThumbnail(result));
                    scannedImageHelper.PostProcessStep2(image, result, ScanProfile, ScanParams, 1);
                    yield return image;
                }
            }
        }
    }
}
