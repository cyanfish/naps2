/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2016       Alexander Rabenstein
    Copyright (C) 2012-2016  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NAPS2.Scan.Images.Transforms;
using ZXing;

namespace NAPS2.Scan.Images
{
    internal static class ScannedImageHelper
    {
        public static string SaveSmallestBitmap(Bitmap sourceImage, ScanBitDepth bitDepth, bool highQuality, int quality, out ImageFormat imageFormat)
        {
            // Store the image in as little space as possible
            if (bitDepth == ScanBitDepth.BlackWhite)
            {
                // Convert to a 1-bit bitmap before saving to help compression
                // This is lossless and takes up minimal storage (best of both worlds), so highQuality is irrelevant
                using (var bitmap = BitmapHelper.CopyToBpp(sourceImage, 1))
                {
                    imageFormat = ImageFormat.Png;
                    return EncodePng(bitmap);
                }
                // Note that if a black and white image comes from native WIA, bitDepth is unknown,
                // so the image will be png-encoded below instead of using a 1-bit bitmap
            }
            else if (highQuality)
            {
                // Store as PNG
                // Lossless, but some images (color/grayscale) take up lots of storage
                imageFormat = ImageFormat.Png;
                return EncodePng(sourceImage);
            }
            else if (Equals(sourceImage.RawFormat, ImageFormat.Jpeg))
            {
                // Store as JPEG
                // Since the image was originally in JPEG format, PNG is unlikely to have size benefits
                imageFormat = ImageFormat.Jpeg;
                return EncodeJpeg(sourceImage, quality);
            }
            else
            {
                // Store as PNG/JPEG depending on which is smaller
                var pngEncoded = EncodePng(sourceImage);
                var jpegEncoded = EncodeJpeg(sourceImage, quality);
                if (new FileInfo(pngEncoded).Length <= new FileInfo(jpegEncoded).Length)
                {
                    // Probably a black and white image (from native WIA, so bitDepth is unknown), which PNG compresses well vs. JPEG
                    File.Delete(jpegEncoded);
                    imageFormat = ImageFormat.Png;
                    return pngEncoded;
                }
                else
                {
                    // Probably a color or grayscale image, which JPEG compresses well vs. PNG
                    File.Delete(pngEncoded);
                    imageFormat = ImageFormat.Jpeg;
                    return jpegEncoded;
                }
            }
        }

        private static string GetTempFilePath()
        {
            return Path.Combine(Paths.Temp, Path.GetRandomFileName());
        }

        private static string EncodePng(Bitmap bitmap)
        {
            var tempFilePath = GetTempFilePath();
            bitmap.Save(tempFilePath, ImageFormat.Png);
            return tempFilePath;
        }

        private static string EncodeJpeg(Bitmap bitmap, int quality)
        {
            var tempFilePath = GetTempFilePath();
            if (quality == -1)
            {
                bitmap.Save(tempFilePath, ImageFormat.Jpeg);
            }
            else
            {
                quality = Math.Max(Math.Min(quality, 100), 0);
                var encoder = ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == ImageFormat.Jpeg.Guid);
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                bitmap.Save(tempFilePath, encoder, encoderParams);
            }
            return tempFilePath;
        }

        public static Bitmap PostProcessStep1(Image output, ScanProfile profile)
        {
            double scaleFactor = 1;
            if (!profile.UseNativeUI)
            {
                scaleFactor = profile.AfterScanScale.ToIntScaleFactor();
            }
            var result = ImageScaleHelper.ScaleImage(output, scaleFactor);

            if (!profile.UseNativeUI && profile.ForcePageSize)
            {
                float width = output.Width / output.HorizontalResolution;
                float height = output.Height / output.VerticalResolution;
                if (float.IsNaN(width) || float.IsNaN(height))
                {
                    width = output.Width;
                    height = output.Height;
                }
                PageDimensions pageDimensions = profile.PageSize.PageDimensions() ?? profile.CustomPageSize;
                if (pageDimensions.Width > pageDimensions.Height && width < height)
                {
                    // Flip dimensions
                    result = new CropTransform
                    {
                        Right = (int)((width - (float)pageDimensions.HeightInInches()) * output.HorizontalResolution),
                        Bottom = (int)((height - (float)pageDimensions.WidthInInches()) * output.VerticalResolution)
                    }.Perform(result);
                    // result.SetResolution((float)(output.Width / pageDimensions.HeightInInches()), (float)(output.Height / pageDimensions.WidthInInches()));
                }
                else
                {
                    result = new CropTransform
                    {
                        Right = (int)((width - (float)pageDimensions.WidthInInches()) * output.HorizontalResolution),
                        Bottom = (int)((height - (float)pageDimensions.HeightInInches()) * output.VerticalResolution)
                    }.Perform(result);
                    //result.SetResolution((float)(output.Width / pageDimensions.WidthInInches()), (float)(output.Height / pageDimensions.HeightInInches()));
                }
            }

            return result;
        }

        public static void PostProcessStep2(ScannedImage image, Bitmap bitmap, ScanProfile profile, ScanParams scanParams, int pageNumber)
        {
            if (!profile.UseNativeUI && profile.BrightnessContrastAfterScan)
            {
                if (profile.Brightness != 0)
                {
                    AddTransformAndUpdateThumbnail(image, new BrightnessTransform { Brightness = profile.Brightness });
                }
                if (profile.Contrast != 0)
                {
                    AddTransformAndUpdateThumbnail(image, new TrueContrastTransform { Contrast = profile.Contrast });
                }
            }
            if (profile.FlipDuplexedPages && pageNumber % 2 == 0)
            {
                AddTransformAndUpdateThumbnail(image, new RotationTransform(RotateFlipType.Rotate180FlipNone));
            }
            if (scanParams.DetectPatchCodes && image.PatchCode == PatchCode.None)
            {
                IBarcodeReader reader = new BarcodeReader();
                var barcodeResult = reader.Decode(bitmap);
                if (barcodeResult != null)
                {
                    switch (barcodeResult.Text)
                    {
                        case "PATCH1":
                            image.PatchCode = PatchCode.Patch1;
                            break;
                        case "PATCH2":
                            image.PatchCode = PatchCode.Patch2;
                            break;
                        case "PATCH3":
                            image.PatchCode = PatchCode.Patch3;
                            break;
                        case "PATCH4":
                            image.PatchCode = PatchCode.Patch4;
                            break;
                        case "PATCH6":
                            image.PatchCode = PatchCode.Patch6;
                            break;
                        case "PATCHT":
                            image.PatchCode = PatchCode.PatchT;
                            break;
                    }
                }
            }
        }

        private static void AddTransformAndUpdateThumbnail(ScannedImage image, Transform transform)
        {
            image.AddTransform(transform);
            var thumbnail = image.GetThumbnail(null);
            if (thumbnail != null)
            {
                image.SetThumbnail(transform.Perform(thumbnail));
            }
        }
    }
}
