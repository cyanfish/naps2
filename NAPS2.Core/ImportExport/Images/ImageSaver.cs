/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2015  Ben Olden-Cooligan

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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.ImportExport.Images
{
    public class ImageSaver
    {
        private readonly IErrorOutput errorOutput;
        private readonly FileNameSubstitution fileNameSubstitution;
        private readonly ImageSettingsContainer imageSettingsContainer;
        private readonly IOverwritePrompt overwritePrompt;

        public ImageSaver(IErrorOutput errorOutput, FileNameSubstitution fileNameSubstitution, ImageSettingsContainer imageSettingsContainer, IOverwritePrompt overwritePrompt)
        {
            this.errorOutput = errorOutput;
            this.fileNameSubstitution = fileNameSubstitution;
            this.imageSettingsContainer = imageSettingsContainer;
            this.overwritePrompt = overwritePrompt;
        }

        /// <summary>
        /// Saves the provided collection of images to a file with the given name. The image type is inferred from the file extension.
        /// If multiple images are provided, they will be saved to files with numeric identifiers, e.g. img1.jpg, img2.jpg, etc..
        /// </summary>
        /// <param name="fileName">The name of the file to save. For multiple images, this is modified by appending a number before the extension.</param>
        /// <param name="images">The collection of images to save.</param>
        public void SaveImages(string fileName, DateTime dateTime, ICollection<IScannedImage> images)
        {
            try
            {
                var subFileName = fileNameSubstitution.SubstituteFileName(fileName, dateTime);
                ImageFormat format = GetImageFormat(subFileName);

                if (Equals(format, ImageFormat.Tiff))
                {
                    if (File.Exists(subFileName))
                    {
                        if (overwritePrompt.ConfirmOverwrite(subFileName) != DialogResult.Yes)
                        {
                            return;
                        }
                    }
                    Image[] bitmaps = images.Select(x => (Image)x.GetImage()).ToArray();
                    TiffHelper.SaveMultipage(bitmaps, subFileName);
                    foreach (Image bitmap in bitmaps)
                    {
                        bitmap.Dispose();
                    }
                    return;
                }

                int i = 0;
                int digits = (int)Math.Floor(Math.Log10(images.Count)) + 1;
                foreach (IScannedImage img in images)
                {
                    if (images.Count == 1 && File.Exists(subFileName))
                    {
                        var dialogResult = overwritePrompt.ConfirmOverwrite(subFileName);
                        if (dialogResult == DialogResult.No)
                        {
                            continue;
                        }
                        if (dialogResult == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                    using (Bitmap baseImage = img.GetImage())
                    {
                        if (images.Count == 1)
                        {
                            DoSaveImage(baseImage, subFileName, format);
                        }
                        else
                        {
                            var fileNameN = fileNameSubstitution.SubstituteFileName(fileName, dateTime, true, i++, digits);
                            DoSaveImage(baseImage, fileNameN, format);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                errorOutput.DisplayError(MiscResources.DontHavePermission);
            }
            catch (IOException ex)
            {
                Log.ErrorException(MiscResources.ErrorSaving, ex);
                errorOutput.DisplayError(MiscResources.ErrorSaving);
            }
        }

        private void DoSaveImage(Bitmap image, string path, ImageFormat format)
        {
            if (Equals(format, ImageFormat.Jpeg))
            {
                var quality = Math.Max(Math.Min(imageSettingsContainer.ImageSettings.JpegQuality, 100), 0);
                var encoder = ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == ImageFormat.Jpeg.Guid);
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                image.Save(path, encoder, encoderParams);
            }
            else
            {
                image.Save(path, format);
            }
        }

        private static ImageFormat GetImageFormat(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            Debug.Assert(extension != null);
            switch (extension.ToLower())
            {
                case ".bmp":
                    return ImageFormat.Bmp;
                case ".emf":
                    return ImageFormat.Emf;
                case ".gif":
                    return ImageFormat.Gif;
                case ".ico":
                    return ImageFormat.Icon;
                case ".jpg":
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                case ".tif":
                case ".tiff":
                    return ImageFormat.Tiff;
                case ".wmf":
                    return ImageFormat.Wmf;
                default:
                    return ImageFormat.Jpeg;
            }
        }
    }
}
