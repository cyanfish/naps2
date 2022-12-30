using NAPS2.Images.Bitwise;

namespace NAPS2.Scan.Internal;

internal class RemotePostProcessor : IRemotePostProcessor
{
    private readonly ScanningContext _scanningContext;

    public RemotePostProcessor(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
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

    public ProcessedImage? PostProcess(IMemoryImage image, ScanOptions options, PostProcessingContext postProcessingContext)
    {
        image = DoInitialTransforms(image, options);
        try
        {
            if (options.ExcludeBlankPages)
            {
                var op = new BlankDetectionImageOp(options.BlankPageWhiteThreshold, options.BlankPageCoverageThreshold);
                op.Perform(image);
                if (op.IsBlank)
                {
                    // TODO: Consider annotating the image as blank via postprocessingdata rather than excluding here
                    // TODO: In theory we might want to add some functionality to allow the user to correct blank detection
                    return null;
                }
            }

            var bitDepth = options.UseNativeUI ? BitDepth.Color : options.BitDepth;
            var scannedImage = _scanningContext.CreateProcessedImage(image, bitDepth, options.MaxQuality,
                options.Quality);
            DoRevertibleTransforms(ref scannedImage, ref image, options, postProcessingContext);
            postProcessingContext.TempPath = SaveForBackgroundOcr(image, options);
            // TODO: We need to attach the thumbnail to the scanned image
            return scannedImage;
        }
        finally
        {
            // Can't use "using" as the image reference could change
            image.Dispose();
        }
    }

    private IMemoryImage DoInitialTransforms(IMemoryImage original, ScanOptions options)
    {
        if (!PlatformCompat.System.CanUseWin32 && options.BitDepth == BitDepth.BlackAndWhite)
        {
            // TODO: Don't do this here, do it where BitmapHelper is used or something
            original = original.PerformTransform(new BlackWhiteTransform(-options.Brightness));
        }

        var scaled = original;
        if (!options.UseNativeUI && options.ScaleRatio > 1)
        {
            var scaleFactor = 1.0 / options.ScaleRatio;
            scaled = scaled.PerformTransform(new ScaleTransform(scaleFactor));
        }

        if (!options.UseNativeUI && (options.StretchToPageSize || options.CropToPageSize))
        {
            float width = original.Width / original.HorizontalResolution;
            float height = original.Height / original.VerticalResolution;
            if (float.IsNaN(width) || float.IsNaN(height))
            {
                width = original.Width;
                height = original.Height;
            }

            if (options.PageSize!.Width > options.PageSize.Height && width < height)
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
                    scaled.SetResolution((float) (original.Width / options.PageSize.WidthInInches), (float) (original.Height / options.PageSize.HeightInInches));
                }
            }
        }

        scaled.UpdateLogicalPixelFormat();
        return scaled;
    }

    // TODO: This is more than just transforms.
    private void DoRevertibleTransforms(ref ProcessedImage processedImage, ref IMemoryImage image, ScanOptions options, PostProcessingContext postProcessingContext)
    {
        var data = processedImage.PostProcessingData;
        if (options.ThumbnailSize.HasValue)
        {
            data = data with
            {
                // TODO: Maybe there's a way we can do this without needing to clone
                Thumbnail = image.Clone().PerformTransform(new ThumbnailTransform(options.ThumbnailSize.Value))
            };
        }

        if (!options.UseNativeUI && options.BrightnessContrastAfterScan)
        {
            processedImage = AddTransformAndUpdateThumbnail(processedImage, ref image, new BrightnessTransform(options.Brightness), options);
            processedImage = AddTransformAndUpdateThumbnail(processedImage, ref image, new TrueContrastTransform(options.Contrast), options);
        }

        // TODO: Do we need to restrict this to only when an actual duplex scan is happening?
        if (options.FlipDuplexedPages && postProcessingContext.PageNumber % 2 == 0)
        {
            processedImage = AddTransformAndUpdateThumbnail(processedImage, ref image, new RotationTransform(180), options);
        }

        if (options.AutoDeskew)
        {
            processedImage = AddTransformAndUpdateThumbnail(processedImage, ref image, Deskewer.GetDeskewTransform(image), options);
        }

        if (!data.BarcodeDetection.IsBarcodePresent)
        {
            // Even if barcode detection was attempted previously and failed, image adjustments may improve detection.
            data = data with { BarcodeDetection = BarcodeDetector.Detect(image, options.BarcodeDetectionOptions) };
        }
        data = data with { ThumbnailTransformState = processedImage.TransformState };
        processedImage = processedImage.WithPostProcessingData(data, true);
    }

    private string? SaveForBackgroundOcr(IMemoryImage bitmap, ScanOptions options)
    {
        if (!string.IsNullOrEmpty(options.OcrParams.LanguageCode))
        {
            // TODO: If we use tesseract as a library, this is something that that could potentially improve (i.e. not having to save to disk)
            // But then again, that doesn't make as much sense on systems (i.e. linux) where tesseract would be provided as an external package
            return _scanningContext.SaveToTempFile(bitmap, options.BitDepth);
        }
        return null;
    }

    private ProcessedImage AddTransformAndUpdateThumbnail(ProcessedImage processedImage, ref IMemoryImage image, Transform transform, ScanOptions options)
    {
        if (transform.IsNull)
        {
            return processedImage;
        }
        ProcessedImage transformed = processedImage.WithTransform(transform, true);
        if (options.ThumbnailSize.HasValue)
        {
            // TODO: We may want to do the transform on the original thumbnail, maybe situationally?
            // TODO: Should probably dispose the original image & thumbnail
            // TODO: This should probably be done even without thumbnails, otherwise deskew/barcode might misfire
            // TODO: If we're doing a number of transforms, this is redundant...
            // TODO: So basically we should probably do ONE thumbnail render, after all transforms are determined.
            // TODO: BUT we should have some kind of fast path (not just used here) that moves the thumbnail transform up the transform stack
            // TODO: as long as subsequent transforms are size agnostic (i.e. 90 deg rotation).
            image = image.PerformTransform(transform);
            transformed = transformed.WithPostProcessingData(
                transformed.PostProcessingData with
                {
                    Thumbnail = image.PerformTransform(new ThumbnailTransform(options.ThumbnailSize.Value))
                }, true);
        }
        return transformed;
    }
}