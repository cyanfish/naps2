using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using NAPS2.Ocr;
using NAPS2.Platform;
using NAPS2.Util;

namespace NAPS2.Scan.Experimental.Internal
{
    internal class RemotePostProcessor : IRemotePostProcessor
    {
        private readonly BlankDetector blankDetector;

        public RemotePostProcessor()
            : this(BlankDetector.Default)
        {
        }

        public RemotePostProcessor(BlankDetector blankDetector)
        {
            this.blankDetector = blankDetector;
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

        public ScannedImage PostProcess(IImage image, ScanOptions options, PostProcessingContext postProcessingContext)
        {
            using (image = DoInitialTransforms(image, options))
            {
                if (options.ExcludeBlankPages && blankDetector.IsBlank(image, options.BlankPageWhiteThreshold, options.BlankPageCoverageThreshold))
                {
                    return null;
                }

                var bitDepth = options.UseNativeUI ? BitDepth.Color : options.BitDepth;
                var scannedImage = new ScannedImage(image, bitDepth, options.MaxQuality, options.Quality);
                DoRevertibleTransforms(scannedImage, image, options, postProcessingContext);
                postProcessingContext.TempPath = SaveForBackgroundOcr(image, options);
                return scannedImage;
            }
        }

        private static IImage DoInitialTransforms(IImage original, ScanOptions options)
        {
            if (!PlatformCompat.System.CanUseWin32 && options.BitDepth == BitDepth.BlackAndWhite)
            {
                // TODO: Don't do this here, do it where BitmapHelper is used or something
                original = Transform.Perform(original, new BlackWhiteTransform(-options.Brightness));
            }

            double scaleFactor = 1;
            if (!options.UseNativeUI)
            {
                scaleFactor = options.ScaleRatio;
            }

            var scaled = Transform.Perform(original, new ScaleTransform(scaleFactor));

            if (!options.UseNativeUI && (options.StretchToPageSize || options.CropToPageSize))
            {
                float width = original.Width / original.HorizontalResolution;
                float height = original.Height / original.VerticalResolution;
                if (float.IsNaN(width) || float.IsNaN(height))
                {
                    width = original.Width;
                    height = original.Height;
                }

                if (options.PageSize.Width > options.PageSize.Height && width < height)
                {
                    if (options.CropToPageSize)
                    {
                        scaled = Transform.Perform(scaled, new CropTransform(
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
                        scaled = Transform.Perform(scaled, new CropTransform
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

            return scaled;
        }

        // TODO: This is more than just transforms.
        private void DoRevertibleTransforms(ScannedImage scannedImage, IImage image, ScanOptions options, PostProcessingContext postProcessingContext)
        {
            if (options.ThumbnailSize.HasValue)
            {
                scannedImage.SetThumbnail(Transform.Perform(image, new ThumbnailTransform(options.ThumbnailSize.Value)));
            }
            if (!options.UseNativeUI && options.BrightnessContrastAfterScan)
            {
                if (options.Brightness != 0)
                {
                    AddTransformAndUpdateThumbnail(scannedImage, ref image, new BrightnessTransform(options.Brightness), options);
                }

                if (options.Contrast != 0)
                {
                    AddTransformAndUpdateThumbnail(scannedImage, ref image, new TrueContrastTransform(options.Contrast), options);
                }
            }

            if (options.FlipDuplexedPages && postProcessingContext.PageNumber % 2 == 0)
            {
                AddTransformAndUpdateThumbnail(scannedImage, ref image, new RotationTransform(180), options);
            }

            if (options.AutoDeskew)
            {
                var op = new DeskewOperation();
                if (op.Start(new[] { scannedImage }, new DeskewParams { ThumbnailSize = options.ThumbnailSize }))
                {
                    // TODO: How to do this, if at all?
                    //operationProgress.ShowProgress(op);
                    op.Wait();
                }
            }

            if (options.DetectPatchCodes && scannedImage.PatchCode == PatchCode.None)
            {
                scannedImage.PatchCode = PatchCodeDetector.Detect(image);
            }
        }

        public string SaveForBackgroundOcr(IImage bitmap, ScanOptions options)
        {
            if (options.DoOcr)
            {
                var fileStorage = StorageManager.Convert<FileStorage>(bitmap, new StorageConvertParams { Temporary = true });
                // TODO: Maybe return the storage rather than the path
                return fileStorage.FullPath;
            }
            return null;
        }

        private void AddTransformAndUpdateThumbnail(ScannedImage scannedImage, ref IImage image, Transform transform, ScanOptions options)
        {
            scannedImage.AddTransform(transform);
            if (options.ThumbnailSize.HasValue)
            {
                var thumbnail = scannedImage.GetThumbnail();
                if (thumbnail != null)
                {
                    image = Transform.Perform(image, transform);
                    scannedImage.SetThumbnail(Transform.Perform(image, new ThumbnailTransform(options.ThumbnailSize.Value)));
                }
            }
        }
    }
}