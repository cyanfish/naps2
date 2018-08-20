using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.Util;
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
            yield return Transfer();
        }

        private ScannedImage Transfer()
        {
            Stream stream;
            if (ScanParams.NoUI)
            {
                stream = saneWrapper.ScanOne(ScanDevice.ID, ScanProfile.KeyValueOptions, null);
            }
            else
            {
                var form = formFactory.Create<FScanProgress>();
                form.Transfer = () => saneWrapper.ScanOne(ScanDevice.ID, ScanProfile.KeyValueOptions, form.OnProgress);
                form.Show();

                if (form.Exception != null)
                {
                    form.Exception.PreserveStackTrace();
                    throw form.Exception;
                }
                if (form.DialogResult == DialogResult.Cancel)
                {
                    return null;
                }

                stream = form.ImageStream;
            }
            using (stream)
            using (var output = Image.FromStream(stream))
            using (var result = scannedImageHelper.PostProcessStep1(output, ScanProfile))
            {
                if (blankDetector.ExcludePage(result, ScanProfile))
                {
                    return null;
                }

                // TODO: Set bit depth correctly
                var image = new ScannedImage(result, ScanProfile.BitDepth, ScanProfile.MaxQuality, ScanProfile.Quality);
                image.SetThumbnail(thumbnailRenderer.RenderThumbnail(result));
                scannedImageHelper.PostProcessStep2(image, result, ScanProfile, ScanParams, 1);
                return image;
            }
        }
    }
}
