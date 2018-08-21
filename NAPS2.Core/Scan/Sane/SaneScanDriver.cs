using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Platform;
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

        public override bool IsSupported => PlatformCompat.System.IsSaneDriverSupported;

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
            var options = GetOptions();
            var img = Transfer(options);
            if (img != null)
            {
                yield return img;
            }
        }

        private KeyValueScanOptions GetOptions()
        {
            var saneOptions = saneWrapper.GetOptions(ScanDevice.ID);
            var options = new KeyValueScanOptions(ScanProfile.KeyValueOptions ?? new KeyValueScanOptions());

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
                    options[name] = value.ToString(CultureInfo.InvariantCulture);
                    return true;
                }
                return false;
            }

            bool IsFlatbedChoice(string choice) => choice.IndexOf("flatbed", StringComparison.InvariantCultureIgnoreCase) >= 0;
            bool IsFeederChoice(string choice) => new[] { "adf", "feeder", "simplex" }.Any(x => choice.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) >= 0);
            bool IsDuplexChoice(string choice) => choice.IndexOf("duplex", StringComparison.InvariantCultureIgnoreCase) >= 0;

            if (ScanProfile.PaperSource == ScanSource.Glass)
            {
                ChooseStringOption("--source", IsFlatbedChoice);
            }
            else if (ScanProfile.PaperSource == ScanSource.Feeder)
            {
                if (!ChooseStringOption("--source", x => IsFeederChoice(x) && !IsDuplexChoice(x)))
                {
                    ChooseStringOption("--source", IsFeederChoice);
                }
            }
            else if (ScanProfile.PaperSource == ScanSource.Duplex)
            {
                if (!ChooseStringOption("--source", IsDuplexChoice))
                {
                    ChooseStringOption("--source", IsFeederChoice);
                }
            }

            if (ScanProfile.BitDepth == ScanBitDepth.C24Bit)
            {
                ChooseStringOption("--mode", x => x == "Color");
                ChooseNumericOption("--depth", 8);
            }
            else if (ScanProfile.BitDepth == ScanBitDepth.Grayscale)
            {
                ChooseStringOption("--mode", x => x == "Gray");
                ChooseNumericOption("--depth", 8);
            }
            else if (ScanProfile.BitDepth == ScanBitDepth.BlackWhite)
            {
                if (!ChooseStringOption("--mode", x => x == "Lineart"))
                {
                    ChooseStringOption("--mode", x => x == "Halftone");
                }
                ChooseNumericOption("--depth", 1);
            }

            var pageDimens = ScanProfile.PageSize.PageDimensions() ?? ScanProfile.CustomPageSize;
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
                    if (ScanProfile.PageAlign == ScanHorizontalAlign.Center)
                    {
                        ChooseNumericOption("-l", (maxWidth.Value - width) / 2);
                    }
                    else if (ScanProfile.PageAlign == ScanHorizontalAlign.Right)
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

            var dpi = ScanProfile.Resolution.ToIntDpi();
            if (!ChooseNumericOption("--resolution", dpi))
            {
                ChooseNumericOption("--x-resolution", dpi);
                ChooseNumericOption("--y-resolution", dpi);
            }

            return options;
        }

        private ScannedImage Transfer(KeyValueScanOptions options)
        {
            Stream stream;
            if (ScanParams.NoUI)
            {
                stream = saneWrapper.ScanOne(ScanDevice.ID, options, null);
            }
            else
            {
                var form = formFactory.Create<FScanProgress>();
                form.Transfer = () => saneWrapper.ScanOne(ScanDevice.ID, options, form.OnProgress);
                form.PageNumber = 1;
                form.ShowDialog();

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
            using (var result = scannedImageHelper.PostProcessStep1(output, ScanProfile, false))
            {
                if (blankDetector.ExcludePage(result, ScanProfile))
                {
                    return null;
                }

                // TODO: Set bit depth correctly
                var image = new ScannedImage(result, ScanProfile.BitDepth, ScanProfile.MaxQuality, ScanProfile.Quality);
                image.SetThumbnail(thumbnailRenderer.RenderThumbnail(result));
                scannedImageHelper.PostProcessStep2(image, result, ScanProfile, ScanParams, 1, false);
                return image;
            }
        }
    }
}
