using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Logging;
using NAPS2.Platform;
using NAPS2.Scan.Exceptions;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.Scan.Sane
{
    public class SaneScanDriver : ScanDriverBase
    {
        public const string DRIVER_NAME = "sane";

        private static readonly Dictionary<string, SaneOptionCollection> SaneOptionCache = new Dictionary<string, SaneOptionCollection>();

        private readonly SaneWrapper saneWrapper;
        private readonly IFormFactory formFactory;
        private readonly BlankDetector blankDetector;
        private readonly ScannedImageHelper scannedImageHelper;

        public SaneScanDriver(SaneWrapper saneWrapper, IFormFactory formFactory, BlankDetector blankDetector, ScannedImageHelper scannedImageHelper)
        {
            this.saneWrapper = saneWrapper;
            this.formFactory = formFactory;
            this.blankDetector = blankDetector;
            this.scannedImageHelper = scannedImageHelper;
        }

        public override string DriverName => DRIVER_NAME;

        public override bool IsSupported => PlatformCompat.System.IsSaneDriverSupported;

        protected override List<ScanDevice> GetDeviceListInternal(ScanProfile scanProfile)
        {
            return saneWrapper.GetDeviceList().ToList();
        }

        protected override async Task ScanInternal(ScannedImageSink sink, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent, CancellationToken cancelToken)
        {
            // TODO: Test ADF
            var options = new Lazy<KeyValueScanOptions>(() => GetOptions(scanProfile, scanDevice));
            int pageNumber = 1;
            var (img, done) = await Transfer(options, pageNumber, scanProfile, scanParams, scanDevice, cancelToken);
            if (img != null)
            {
                sink.PutImage(img);
            }

            if (!done && scanProfile.PaperSource != ScanSource.Glass)
            {
                try
                {
                    while (true)
                    {
                        (img, done) = await Transfer(options, ++pageNumber, scanProfile, scanParams, scanDevice, cancelToken);
                        if (done)
                        {
                            break;
                        }
                        if (img != null)
                        {
                            sink.PutImage(img);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.ErrorException("Error in SANE. This may be a normal ADF termination.", e);
                }
            }
        }

        private KeyValueScanOptions GetOptions(ScanProfile scanProfile, ScanDevice scanDevice)
        {
            var saneOptions = SaneOptionCache.GetOrSet(scanDevice.ID, () => saneWrapper.GetOptions(scanDevice.ID));
            var options = new KeyValueScanOptions(scanProfile.KeyValueOptions ?? new KeyValueScanOptions());

            bool ChooseStringOption(string name, Func<string, bool> match)
            {
                var opt = saneOptions.Get(name);
                var choice = opt?.StringList?.FirstOrDefault(match);
                if (choice != null)
                {
                    options[name] = choice;
                    return true;
                }
                return false;
            }

            bool ChooseNumericOption(string name, decimal value)
            {
                var opt = saneOptions.Get(name);
                if (opt?.ConstraintType == SaneConstraintType.WordList)
                {
                    var choice = opt.WordList?.OrderBy(x => Math.Abs(x - value)).FirstOrDefault();
                    if (choice != null)
                    {
                        options[name] = choice.Value.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                }
                else if (opt?.ConstraintType == SaneConstraintType.Range)
                {
                    if (value < opt.Range.Min)
                    {
                        value = opt.Range.Min;
                    }
                    if (value > opt.Range.Max)
                    {
                        value = opt.Range.Max;
                    }
                    if (opt.Range.Quant != 0)
                    {
                        var mod = (value - opt.Range.Min) % opt.Range.Quant;
                        if (mod != 0)
                        {
                            value = mod < opt.Range.Quant / 2 ? value - mod : value + opt.Range.Quant - mod;
                        }
                    }
                    options[name] = value.ToString("0.#####", CultureInfo.InvariantCulture);
                    return true;
                }
                return false;
            }

            bool IsFlatbedChoice(string choice) => choice.IndexOf("flatbed", StringComparison.InvariantCultureIgnoreCase) >= 0;
            bool IsFeederChoice(string choice) => new[] { "adf", "feeder", "simplex" }.Any(x => choice.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) >= 0);
            bool IsDuplexChoice(string choice) => choice.IndexOf("duplex", StringComparison.InvariantCultureIgnoreCase) >= 0;

            if (scanProfile.PaperSource == ScanSource.Glass)
            {
                ChooseStringOption("--source", IsFlatbedChoice);
            }
            else if (scanProfile.PaperSource == ScanSource.Feeder)
            {
                if (!ChooseStringOption("--source", x => IsFeederChoice(x) && !IsDuplexChoice(x)) &&
                    !ChooseStringOption("--source", IsFeederChoice) &&
                    !ChooseStringOption("--source", IsDuplexChoice))
                {
                    throw new NoFeederSupportException();
                }
            }
            else if (scanProfile.PaperSource == ScanSource.Duplex)
            {
                if (!ChooseStringOption("--source", IsDuplexChoice))
                {
                    throw new NoDuplexSupportException();
                }
            }

            if (scanProfile.BitDepth == ScanBitDepth.C24Bit)
            {
                ChooseStringOption("--mode", x => x == "Color");
                ChooseNumericOption("--depth", 8);
            }
            else if (scanProfile.BitDepth == ScanBitDepth.Grayscale)
            {
                ChooseStringOption("--mode", x => x == "Gray");
                ChooseNumericOption("--depth", 8);
            }
            else if (scanProfile.BitDepth == ScanBitDepth.BlackWhite)
            {
                if (!ChooseStringOption("--mode", x => x == "Lineart"))
                {
                    ChooseStringOption("--mode", x => x == "Halftone");
                }
                ChooseNumericOption("--depth", 1);
                ChooseNumericOption("--threshold", (-scanProfile.Brightness + 1000) / 20m);
            }

            var pageDimens = scanProfile.PageSize.PageDimensions() ?? scanProfile.CustomPageSize;
            if (pageDimens != null)
            {
                var width = pageDimens.WidthInMm();
                var height = pageDimens.HeightInMm();
                ChooseNumericOption("-x", width);
                ChooseNumericOption("-y", height);
                var maxWidth = saneOptions.Get("-l")?.Range?.Max;
                var maxHeight = saneOptions.Get("-t")?.Range?.Max;
                if (maxWidth != null)
                {
                    if (scanProfile.PageAlign == ScanHorizontalAlign.Center)
                    {
                        ChooseNumericOption("-l", (maxWidth.Value - width) / 2);
                    }
                    else if (scanProfile.PageAlign == ScanHorizontalAlign.Right)
                    {
                        ChooseNumericOption("-l", maxWidth.Value - width);
                    }
                    else
                    {
                        ChooseNumericOption("-l", 0);
                    }
                }
                if (maxHeight != null)
                {
                    ChooseNumericOption("-t", 0);
                }
            }

            var dpi = scanProfile.Resolution.ToIntDpi();
            if (!ChooseNumericOption("--resolution", dpi))
            {
                ChooseNumericOption("--x-resolution", dpi);
                ChooseNumericOption("--y-resolution", dpi);
            }

            return options;
        }

        private async Task<(ScannedImage, bool)> Transfer(Lazy<KeyValueScanOptions> options, int pageNumber, ScanProfile scanProfile, ScanParams scanParams, ScanDevice scanDevice, CancellationToken cancelToken)
        {
            return await Task.Factory.StartNew(() =>
            {
                Stream stream;
                if (scanParams.NoUI)
                {
                    stream = saneWrapper.ScanOne(scanDevice.ID, options.Value, null, cancelToken);
                }
                else
                {
                    var form = formFactory.Create<FScanProgress>();
                    var unifiedCancelToken = CancellationTokenSource.CreateLinkedTokenSource(form.CancelToken, cancelToken).Token;
                    form.Transfer = () => saneWrapper.ScanOne(scanDevice.ID, options.Value, form.OnProgress, unifiedCancelToken);
                    form.PageNumber = pageNumber;
                    ((FormBase)Application.OpenForms[0]).SafeInvoke(() => form.ShowDialog());

                    if (form.Exception != null)
                    {
                        form.Exception.PreserveStackTrace();
                        throw form.Exception;
                    }
                    if (form.DialogResult == DialogResult.Cancel)
                    {
                        return (null, true);
                    }

                    stream = form.ImageStream;
                }
                if (stream == null)
                {
                    return (null, true);
                }
                using (stream)
                using (var output = StorageManager.ImageFactory.Decode(stream, ".bmp"))
                using (var result = scannedImageHelper.PostProcessStep1(output, scanProfile, false))
                {
                    if (blankDetector.ExcludePage(result, scanProfile))
                    {
                        return (null, false);
                    }

                    // By converting to 1bpp here we avoid the Win32 call in the BitmapHelper conversion
                    // This converter also has the side effect of working even if the scanner doesn't support Lineart
                    using (var encoded = scanProfile.BitDepth == ScanBitDepth.BlackWhite ? Transform.Perform(result, new BlackWhiteTransform(-scanProfile.Brightness)) : result)
                    {
                        var image = new ScannedImage(encoded, scanProfile.BitDepth, scanProfile.MaxQuality, scanProfile.Quality);
                        scannedImageHelper.PostProcessStep2(image, result, scanProfile, scanParams, 1, false);
                        string tempPath = scannedImageHelper.SaveForBackgroundOcr(result, scanParams);
                        scannedImageHelper.RunBackgroundOcr(image, scanParams, tempPath);
                        return (image, false);
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
