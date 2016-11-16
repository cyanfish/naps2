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
using System.Threading;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.ImportExport.Images
{
    public class SaveImagesOperation : OperationBase
    {
        private readonly FileNamePlaceholders fileNamePlaceholders;
        private readonly ImageSettingsContainer imageSettingsContainer;
        private readonly IOverwritePrompt overwritePrompt;
        private readonly ThreadFactory threadFactory;

        private bool cancel;
        private Thread thread;

        public SaveImagesOperation(FileNamePlaceholders fileNamePlaceholders, ImageSettingsContainer imageSettingsContainer, IOverwritePrompt overwritePrompt, ThreadFactory threadFactory)
        {
            this.fileNamePlaceholders = fileNamePlaceholders;
            this.imageSettingsContainer = imageSettingsContainer;
            this.overwritePrompt = overwritePrompt;
            this.threadFactory = threadFactory;

            ProgressTitle = MiscResources.SaveImagesProgress;
            AllowCancel = true;
        }

        public string FirstFileSaved { get; private set; }

        /// <summary>
        /// Saves the provided collection of images to a file with the given name. The image type is inferred from the file extension.
        /// If multiple images are provided, they will be saved to files with numeric identifiers, e.g. img1.jpg, img2.jpg, etc..
        /// </summary>
        /// <param name="fileName">The name of the file to save. For multiple images, this is modified by appending a number before the extension.</param>
        /// <param name="dateTime"></param>
        /// <param name="images">The collection of images to save.</param>
        /// <param name="batch"></param>
        public bool Start(string fileName, DateTime dateTime, List<ScannedImage> images, bool batch = false)
        {
            Status = new OperationStatus
            {
                MaxProgress = images.Count
            };
            cancel = false;

            thread = threadFactory.StartThread(() =>
            {
                try
                {
                    var subFileName = fileNamePlaceholders.SubstitutePlaceholders(fileName, dateTime, batch);
                    if (Directory.Exists(subFileName))
                    {
                        // Not supposed to be a directory, but ok...
                        fileName = Path.Combine(subFileName, "$(n).jpg");
                        subFileName = fileNamePlaceholders.SubstitutePlaceholders(fileName, dateTime, batch);
                    }
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
                        Status.StatusText = string.Format(MiscResources.SavingFormat, Path.GetFileName(subFileName));
                        Status.Success = TiffHelper.SaveMultipage(images, subFileName, j =>
                        {
                            Status.CurrentProgress = j;
                            InvokeStatusChanged();
                            return !cancel;
                        });
                        FirstFileSaved = subFileName;
                        return;
                    }

                    int i = 0;
                    int digits = (int) Math.Floor(Math.Log10(images.Count)) + 1;
                    foreach (ScannedImage img in images)
                    {
                        if (cancel)
                        {
                            return;
                        }
                        Status.CurrentProgress = i;
                        InvokeStatusChanged();

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
                                Status.StatusText = string.Format(MiscResources.SavingFormat, Path.GetFileName(subFileName));
                                InvokeStatusChanged();
                                DoSaveImage(baseImage, subFileName, format);
                                FirstFileSaved = subFileName;
                            }
                            else
                            {
                                var fileNameN = fileNamePlaceholders.SubstitutePlaceholders(fileName, dateTime, true, i,
                                    digits);
                                Status.StatusText = string.Format(MiscResources.SavingFormat, Path.GetFileName(fileNameN));
                                InvokeStatusChanged();
                                DoSaveImage(baseImage, fileNameN, format);

                                if (i == 0)
                                {
                                    FirstFileSaved = fileNameN;
                                }
                            }
                        }
                        i++;
                    }

                    Status.Success = FirstFileSaved != null;
                }
                catch (UnauthorizedAccessException ex)
                {
                    InvokeError(MiscResources.DontHavePermission, ex);
                }
                catch (Exception ex)
                {
                    Log.ErrorException(MiscResources.ErrorSaving, ex);
                    InvokeError(MiscResources.ErrorSaving, ex);
                }
                finally
                {
                    GC.Collect();
                    InvokeFinished();
                }
            });

            return true;
        }

        private void DoSaveImage(Bitmap image, string path, ImageFormat format)
        {
            PathHelper.EnsureParentDirExists(path);
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

        public override void Cancel()
        {
            cancel = true;
        }

        public void WaitUntilFinished(bool throwOnError = true)
        {
            thread.Join();
            if (throwOnError && LastError != null)
            {
                throw new Exception(LastError.ErrorMessage, LastError.Exception);
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
