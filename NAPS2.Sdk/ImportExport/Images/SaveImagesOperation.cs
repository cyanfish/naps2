using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Operation;
using NAPS2.Images;
using NAPS2.Util;

namespace NAPS2.ImportExport.Images
{
    // TODO: Avoid GDI dependency
    public class SaveImagesOperation : OperationBase
    {
        private readonly OverwritePrompt overwritePrompt;
        private readonly BitmapRenderer bitmapRenderer;
        private readonly TiffHelper tiffHelper;

        public SaveImagesOperation(OverwritePrompt overwritePrompt, BitmapRenderer bitmapRenderer, TiffHelper tiffHelper)
        {
            this.overwritePrompt = overwritePrompt;
            this.bitmapRenderer = bitmapRenderer;
            this.tiffHelper = tiffHelper;

            ProgressTitle = MiscResources.SaveImagesProgress;
            AllowCancel = true;
            AllowBackground = true;
        }

        public string FirstFileSaved { get; private set; }

        /// <summary>
        /// Saves the provided collection of images to a file with the given name. The image type is inferred from the file extension.
        /// If multiple images are provided, they will be saved to files with numeric identifiers, e.g. img1.jpg, img2.jpg, etc..
        /// </summary>
        /// <param name="fileName">The name of the file to save. For multiple images, this is modified by appending a number before the extension.</param>
        /// <param name="placeholders"></param>
        /// <param name="images">The collection of images to save.</param>
        /// <param name="batch"></param>
        public bool Start(string fileName, Placeholders placeholders, List<ScannedImage> images, ConfigProvider<ImageSettings> imageSettings, bool batch = false)
        {
            Status = new OperationStatus
            {
                MaxProgress = images.Count
            };

            var snapshots = images.Select(x => x.Preserve()).ToList();
            RunAsync(async () =>
            {
                try
                {
                    var subFileName = placeholders.Substitute(fileName, batch);
                    if (Directory.Exists(subFileName))
                    {
                        // Not supposed to be a directory, but ok...
                        fileName = Path.Combine(subFileName, "$(n).jpg");
                        subFileName = placeholders.Substitute(fileName, batch);
                    }
                    ImageFormat format = GetImageFormat(subFileName);

                    if (Equals(format, ImageFormat.Tiff) && !imageSettings.Get(c => c.SinglePageTiff))
                    {
                        if (File.Exists(subFileName))
                        {
                            if (overwritePrompt.ConfirmOverwrite(subFileName) != DialogResult.Yes)
                            {
                                return false;
                            }
                        }
                        Status.StatusText = string.Format(MiscResources.SavingFormat, Path.GetFileName(subFileName));
                        FirstFileSaved = subFileName;
                        return await tiffHelper.SaveMultipage(snapshots, subFileName, imageSettings.Get(c => c.TiffCompression), OnProgress, CancelToken);
                    }

                    int i = 0;
                    int digits = (int)Math.Floor(Math.Log10(snapshots.Count)) + 1;
                    foreach (ScannedImage.Snapshot snapshot in snapshots)
                    {
                        if (CancelToken.IsCancellationRequested)
                        {
                            return false;
                        }
                        Status.CurrentProgress = i;
                        InvokeStatusChanged();

                        if (snapshots.Count == 1 && File.Exists(subFileName))
                        {
                            var dialogResult = overwritePrompt.ConfirmOverwrite(subFileName);
                            if (dialogResult == DialogResult.No)
                            {
                                continue;
                            }
                            if (dialogResult == DialogResult.Cancel)
                            {
                                return false;
                            }
                        }
                        if (snapshots.Count == 1)
                        {
                            Status.StatusText = string.Format(MiscResources.SavingFormat, Path.GetFileName(subFileName));
                            InvokeStatusChanged();
                            await DoSaveImage(snapshot, subFileName, format, imageSettings);
                            FirstFileSaved = subFileName;
                        }
                        else
                        {
                            var fileNameN = placeholders.Substitute(fileName, true, i,
                                digits);
                            Status.StatusText = string.Format(MiscResources.SavingFormat, Path.GetFileName(fileNameN));
                            InvokeStatusChanged();
                            await DoSaveImage(snapshot, fileNameN, format, imageSettings);

                            if (i == 0)
                            {
                                FirstFileSaved = fileNameN;
                            }
                        }
                        i++;
                    }

                    return FirstFileSaved != null;
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
                    snapshots.ForEach(s => s.Dispose());
                    GC.Collect();
                }
                return false;
            });
            Success.ContinueWith(task =>
            {
                if (task.Result)
                {
                    Log.Event(EventType.SaveImages, new EventParams
                    {
                        Name = MiscResources.SaveImages,
                        Pages = snapshots.Count,
                        FileFormat = Path.GetExtension(fileName)
                    });
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            return true;
        }

        private async Task DoSaveImage(ScannedImage.Snapshot snapshot, string path, ImageFormat format, ConfigProvider<ImageSettings> imageSettings)
        {
            PathHelper.EnsureParentDirExists(path);
            if (Equals(format, ImageFormat.Tiff))
            {
                await tiffHelper.SaveMultipage(new List<ScannedImage.Snapshot> { snapshot }, path, imageSettings.Get(c => c.TiffCompression), (i, j) => { }, CancellationToken.None);
            }
            else if (Equals(format, ImageFormat.Jpeg))
            {
                var quality = imageSettings.Get(c => c.JpegQuality).Clamp(0, 100);
                var encoder = ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == ImageFormat.Jpeg.Guid);
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                // TODO: Something more generic
                using Bitmap bitmap = await bitmapRenderer.Render(snapshot);
                bitmap.Save(path, encoder, encoderParams);
            }
            else
            {
                using Bitmap bitmap = await bitmapRenderer.Render(snapshot);
                bitmap.Save(path, format);
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
