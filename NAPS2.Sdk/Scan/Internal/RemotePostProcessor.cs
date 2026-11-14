using Microsoft.Extensions.Logging;
using NAPS2.Images.Bitwise;

namespace NAPS2.Scan.Internal;

internal class RemotePostProcessor : IRemotePostProcessor
{
    private readonly ScanningContext _scanningContext;
    private readonly ILogger _logger;

    public RemotePostProcessor(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
        _logger = scanningContext.Logger;
    }


    //using (var result = PostProcessStep1(output, scanProfile))
    //{
    //    if (blankDetector.ExcludePage(result, scanProfile))
    //    {
    //        return null;
    //    }

    //    ScanBitDepth bitDepth = scanProfile.UseNativeUI ? ScanBitDepth.C24Bit : scanProfile.BitDepth;
    //    var image = new ScannedImage(result, bitDepth, scanProfile.MaxQuality, scanProfile.Quality);
    //    PostProcessStep2(image, result, scanProfile, scanParams, pageNumber);
    //    string tempPath = SaveForBackgroundOcr(result, scanParams);
    //    RunBackgroundOcr(image, scanParams, tempPath);
    //    return image;
    //}

    public ProcessedImage? PostProcess(IMemoryImage image, ScanOptions options,
        PostProcessingContext postProcessingContext)
    {
        image = DoInitialTransforms(image, options);
        if (options.ExcludeBlankPages)
        {
            var op = new BlankDetectionImageOp(options.BlankPageWhiteThreshold, options.BlankPageCoverageThreshold);
            op.Perform(image);
            if (op.IsBlank)
            {
                // TODO: Consider annotating the image as blank via postprocessingdata rather than excluding here
                // TODO: In theory we might want to add some functionality to allow the user to correct blank detection
                image.Dispose();
                return null;
            }
        }

        using var page = new PostProcessingPage(
            _scanningContext.CreateProcessedImage(image, options.MaxQuality, options.Quality, options.PageSize),
            image);
        DoRevertibleTransforms(page, options, postProcessingContext);
        postProcessingContext.TempPath = SaveForBackgroundOcr(page, options);
        if (options.ThumbnailSize.HasValue)
        {
            // This is done last as it consumes the rendered image
            page.SetPostProcessingData(page.ProcessedImage.PostProcessingData with
            {
                ThumbnailTransformState = page.ProcessedImage.TransformState,
                Thumbnail = page.TakeRendered().PerformTransform(new ThumbnailTransform(options.ThumbnailSize.Value))
            });
        }
        return page.ProcessedImage;
    }

    private IMemoryImage DoInitialTransforms(IMemoryImage original, ScanOptions options)
    {
        if (!options.UseNativeUI && options.BitDepth == BitDepth.BlackAndWhite)
        {
            // Ensure we actually have a black & white image (this is a no-op if we already do)
            original = original.PerformTransform(new BlackWhiteTransform(-options.Brightness));
        }

        var scaled = original;
        if (!options.UseNativeUI && (options.StretchToPageSize || options.CropToPageSize))
        {
            scaled = CropAndStretch(original, options, scaled);
        }

        if (!options.UseNativeUI && options.ScaleRatio > 1)
        {
            var scaleFactor = 1.0 / options.ScaleRatio;
            scaled = scaled.PerformTransform(new ScaleTransform(scaleFactor));
        }

        return scaled;
    }

    private IMemoryImage CropAndStretch(IMemoryImage original, ScanOptions options, IMemoryImage scaled)
    {
        if (original.HorizontalResolution <= 0 || original.VerticalResolution <= 0)
        {
            _logger.LogDebug("Skipping StretchToPageSize/CropToPageSize as there is no resolution data");
            return scaled;
        }

        float width = original.Width / original.HorizontalResolution;
        float height = original.Height / original.VerticalResolution;

        if ((options.PageSize!.Width > options.PageSize.Height) ^ (width > height))
        {
            if (options.CropToPageSize)
            {
                scaled = scaled.PerformTransform(new CropTransform(
                    0,
                    (int) ((width - (float) options.PageSize.HeightInInches) * original.HorizontalResolution),
                    0,
                    (int) ((height - (float) options.PageSize.WidthInInches) * original.VerticalResolution)
                ));
            }
            else
            {
                scaled.SetResolution((float) (original.Width / options.PageSize.HeightInInches),
                    (float) (original.Height / options.PageSize.WidthInInches));
            }
        }
        else
        {
            if (options.CropToPageSize)
            {
                scaled = scaled.PerformTransform(new CropTransform
                (
                    0,
                    (int) ((width - (float) options.PageSize.WidthInInches) * original.HorizontalResolution),
                    0,
                    (int) ((height - (float) options.PageSize.HeightInInches) * original.VerticalResolution)
                ));
            }
            else
            {
                scaled.SetResolution((float) (original.Width / options.PageSize.WidthInInches),
                    (float) (original.Height / options.PageSize.HeightInInches));
            }
        }
        return scaled;
    }

    // TODO: This is more than just transforms.
    private void DoRevertibleTransforms(PostProcessingPage page, ScanOptions options,
        PostProcessingContext postProcessingContext)
    {
        var data = page.ProcessedImage.PostProcessingData with
        {
            PageNumber = postProcessingContext.PageNumber
        };

        if ((!options.UseNativeUI && options.BrightnessContrastAfterScan) ||
            options.Driver is not (Driver.Wia or Driver.Twain))
        {
            page.AddTransform(new BrightnessTransform(options.Brightness));
            page.AddTransform(new TrueContrastTransform(options.Contrast));
        }

        if (options.PaperSource == PaperSource.Duplex)
        {
            data = data with
            {
                PageSide = postProcessingContext.PageNumber % 2 == 0 ? PageSide.Back : PageSide.Front
            };
            if (options.FlipDuplexedPages && data.PageSide == PageSide.Back)
            {
                page.AddTransform(new RotationTransform(180));
            }
        }

        if (options.RotateDegrees != 0)
        {
            page.AddTransform(new RotationTransform(options.RotateDegrees));
        }

        if (options.AutoDeskew)
        {
            page.AddTransform(Deskewer.GetDeskewTransform(page.GetRendered(TransformState.Empty)));
        }

        if (options.AutoCrop)
        {
            // Detect the page on the image with the transforms so far (e.g. deskewing) applied
            var rendered = page.GetRendered();
            var op = new EdgeDetectionImageOp();
            op.Perform(rendered);
            if (op.Left != 0 || op.Top != 0 || op.Right != 0 || op.Bottom != 0)
            {
                page.AddTransform(new CropTransform(
                    op.Left, op.Right, op.Top, op.Bottom, rendered.Width, rendered.Height));
            }
        }

        if (options.CorrectionMode != CorrectionMode.None)
        {
            page.AddTransform(new CorrectionTransform(options.CorrectionMode));
        }

        if (!data.Barcode.IsDetected)
        {
            // Even if barcode detection was attempted previously and failed, image adjustments may improve detection.
            data = data with
            {
                Barcode = options.BarcodeDetectionOptions.DetectBarcodes
                    ? BarcodeDetector.Detect(page.GetRendered(), options.BarcodeDetectionOptions)
                    : Barcode.NoDetection
            };
        }
        page.SetPostProcessingData(data);
    }

    private string? SaveForBackgroundOcr(PostProcessingPage page, ScanOptions options)
    {
        if (!string.IsNullOrEmpty(options.OcrParams.LanguageCode))
        {
            // TODO: If we use tesseract as a library, this is something that that could potentially improve (i.e. not having to save to disk)
            // But then again, that doesn't make as much sense on systems (i.e. linux) where tesseract would be provided as an external package
            return _scanningContext.SaveToTempFile(page.GetRendered());
        }
        return null;
    }
}