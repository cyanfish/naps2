using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Platform;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.Scan.Wia
{
    public class WiaScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "wia";

        private const int MAX_RETRIES = 5;

        private readonly IBlankDetector blankDetector;
        private readonly ScannedImageHelper scannedImageHelper;
        private readonly IFormFactory formFactory;

        public WiaScanDriver(IBlankDetector blankDetector, ScannedImageHelper scannedImageHelper, IFormFactory formFactory)
            : base(formFactory)
        {
            this.blankDetector = blankDetector;
            this.scannedImageHelper = scannedImageHelper;
            this.formFactory = formFactory;
        }

        public override string DriverName => DRIVER_NAME;

        public override bool IsSupported => PlatformCompat.System.IsWiaDriverSupported;

        protected override ScanDevice PromptForDeviceInternal() => WiaApi.PromptForScanDevice();

        protected override List<ScanDevice> GetDeviceListInternal() => WiaApi.GetScanDeviceList();

        protected override async Task ScanInternal(ScannedImageSource.Concrete source)
        {
            using (var eventLoop = new WiaBackgroundEventLoop(ScanProfile, ScanDevice))
            {
                bool supportsFeeder = eventLoop.GetSync(wia => WiaApi.DeviceSupportsFeeder(wia.Device));
                if (ScanProfile.PaperSource != ScanSource.Glass && !supportsFeeder)
                {
                    throw new NoFeederSupportException();
                }
                bool supportsDuplex = eventLoop.GetSync(wia => WiaApi.DeviceSupportsDuplex(wia.Device));
                if (ScanProfile.PaperSource == ScanSource.Duplex && !supportsDuplex)
                {
                    throw new NoDuplexSupportException();
                }
                int pageNumber = 1;
                int retryCount = 0;
                bool retry = false;
                bool done = false;
                do
                {
                    ScannedImage image;
                    try
                    {
                        if (pageNumber > 1 && ScanProfile.WiaDelayBetweenScans)
                        {
                            int delay = (int)(ScanProfile.WiaDelayBetweenScansSeconds.Clamp(0, 30) * 1000);
                            Thread.Sleep(delay);
                        }
                        (image, done) = await TransferImage(eventLoop, pageNumber);
                        pageNumber++;
                        retryCount = 0;
                        retry = false;
                    }
                    catch (ScanDriverException e)
                    {
                        if (ScanProfile.WiaRetryOnFailure && e.InnerException is COMException comError
                            && (uint)comError.ErrorCode == 0x80004005 && retryCount < MAX_RETRIES)
                        {
                            Thread.Sleep(1000);
                            retryCount += 1;
                            retry = true;
                            continue;
                        }
                        throw;
                    }
                    if (image != null)
                    {
                        source.Put(image);
                    }
                } while (!CancelToken.IsCancellationRequested && (retry || !done && ScanProfile.PaperSource != ScanSource.Glass));
            }
        }

        private async Task<(ScannedImage, bool)> TransferImage(WiaBackgroundEventLoop eventLoop, int pageNumber)
        {
            return await Task.Factory.StartNew(() =>
            {
                try
                {
                    ChaosMonkey.MaybeError(0, new COMException("Fail", -2147467259));
                    using (var stream = DoTransfer(pageNumber, eventLoop, WiaApi.Formats.BMP))
                    {
                        if (stream == null)
                        {
                            return (null, true);
                        }

                        using (Image output = Image.FromStream(stream))
                        {
                            using (var result = scannedImageHelper.PostProcessStep1(output, ScanProfile))
                            {
                                if (blankDetector.ExcludePage(result, ScanProfile))
                                {
                                    return (null, false);
                                }

                                ScanBitDepth bitDepth = ScanProfile.UseNativeUI ? ScanBitDepth.C24Bit : ScanProfile.BitDepth;
                                var image = new ScannedImage(result, bitDepth, ScanProfile.MaxQuality, ScanProfile.Quality);
                                scannedImageHelper.PostProcessStep2(image, result, ScanProfile, ScanParams, pageNumber);
                                string tempPath = scannedImageHelper.SaveForBackgroundOcr(result, ScanParams);
                                scannedImageHelper.RunBackgroundOcr(image, ScanParams, tempPath);
                                return (image, false);
                            }
                        }
                    }
                }
                catch (NoPagesException)
                {
                    if (ScanProfile.PaperSource != ScanSource.Glass && pageNumber == 1)
                    {
                        // No pages were in the feeder, so show the user an error
                        throw new NoPagesException();
                    }

                    // At least one page was scanned but now the feeder is empty, so exit normally
                    return (null, true);
                }
            }, TaskCreationOptions.LongRunning);
        }

        private Stream DoTransfer(int pageNumber, WiaBackgroundEventLoop eventLoop, string format)
        {
            var invoker = (FormBase)DialogParent;
            if (eventLoop.GetSync(wia => wia.Item) == null)
            {
                return null;
            }
            if (ScanParams.NoUI)
            {
                return eventLoop.GetSync(wia => WiaApi.Transfer(wia, format, false));
            }
            if (pageNumber == 1)
            {
                // The only downside of the common dialog is that it steals focus.
                // If this is the first page, then the user has just pressed the scan button, so that's not
                // an issue and we can use it and get the benefits of progress display and immediate cancellation.
                return ScanParams.Modal
                    ? eventLoop.GetSync(wia => invoker.InvokeGet(() => WiaApi.Transfer(wia, format, true)))
                    : eventLoop.GetSync(wia => WiaApi.Transfer(wia, format, true));
            }
            // For subsequent pages, we don't want to take focus in case the user has switched applications,
            // so we use the custom form.
            var form = formFactory.Create<FScanProgress>();
            var waitHandle = new AutoResetEvent(false);
            form.PageNumber = pageNumber;
            form.Transfer = () => eventLoop.GetSync(wia => WiaApi.Transfer(wia, format, false));
            form.Closed += (sender, args) => waitHandle.Set();
            if (ScanParams.Modal)
            {
                invoker.Invoke(() => form.ShowDialog(DialogParent));
            }
            else
            {
                invoker.Invoke(() => form.Show(DialogParent));
            }
            waitHandle.WaitOne();
            if (form.Exception != null)
            {
                WiaApi.ThrowDeviceError(form.Exception);
            }
            if (form.DialogResult == DialogResult.Cancel)
            {
                return null;
            }
            return form.ImageStream;
        }
    }
}
