using System.Threading;
using Eto.Drawing;

namespace NAPS2.ImportExport.Images;

// TODO: Cross-platform TIFF
public class SaveImagesOperation : OperationBase
{
    private readonly IOverwritePrompt _overwritePrompt;
    private readonly ITiffHelper _tiffHelper;

    public SaveImagesOperation(IOverwritePrompt overwritePrompt, ITiffHelper tiffHelper)
    {
        _overwritePrompt = overwritePrompt;
        _tiffHelper = tiffHelper;

        ProgressTitle = MiscResources.SaveImagesProgress;
        AllowCancel = true;
        AllowBackground = true;
    }

    public string? FirstFileSaved { get; private set; }

    /// <summary>
    /// Saves the provided collection of images to a file with the given name. The image type is inferred from the file extension.
    /// If multiple images are provided, they will be saved to files with numeric identifiers, e.g. img1.jpg, img2.jpg, etc..
    /// </summary>
    /// <param name="fileName">The name of the file to save. For multiple images, this is modified by appending a number before the extension.</param>
    /// <param name="placeholders"></param>
    /// <param name="images">The collection of images to save.</param>
    /// <param name="batch"></param>
    public bool Start(string fileName, Placeholders placeholders, IList<ProcessedImage> images, ImageSettings imageSettings, bool batch = false)
    {
        Status = new OperationStatus
        {
            MaxProgress = images.Count
        };

        RunAsync(() =>
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

                if (Equals(format, ImageFormat.Tiff) && !imageSettings.SinglePageTiff)
                {
                    if (File.Exists(subFileName))
                    {
                        if (_overwritePrompt.ConfirmOverwrite(subFileName) != OverwriteResponse.Yes)
                        {
                            return false;
                        }
                        if (FileSystemHelper.IsFileInUse(subFileName, out var ex))
                        {
                            InvokeError(MiscResources.FileInUse, ex!);
                            return false;
                        }
                    }
                    Status.StatusText = string.Format(MiscResources.SavingFormat, Path.GetFileName(subFileName));
                    FirstFileSaved = subFileName;
                    return _tiffHelper.SaveMultipage(images, subFileName, imageSettings.TiffCompression, OnProgress, CancelToken);
                }

                int i = 0;
                int digits = (int)Math.Floor(Math.Log10(images.Count)) + 1;
                foreach (ProcessedImage image in images)
                {
                    if (CancelToken.IsCancellationRequested)
                    {
                        return false;
                    }
                    Status.CurrentProgress = i;
                    InvokeStatusChanged();

                    if (images.Count == 1 && File.Exists(subFileName))
                    {
                        var overwriteResponse = _overwritePrompt.ConfirmOverwrite(subFileName);
                        if (overwriteResponse == OverwriteResponse.No)
                        {
                            continue;
                        }
                        if (overwriteResponse == OverwriteResponse.Abort)
                        {
                            return false;
                        }
                        if (FileSystemHelper.IsFileInUse(subFileName, out var ex))
                        {
                            InvokeError(MiscResources.FileInUse, ex!);
                            return false;
                        }
                    }
                    if (images.Count == 1)
                    {
                        Status.StatusText = string.Format(MiscResources.SavingFormat, Path.GetFileName(subFileName));
                        InvokeStatusChanged();
                        DoSaveImage(image, subFileName, format, imageSettings);
                        FirstFileSaved = subFileName;
                    }
                    else
                    {
                        var fileNameN = placeholders.Substitute(fileName, true, i,
                            digits);
                        Status.StatusText = string.Format(MiscResources.SavingFormat, Path.GetFileName(fileNameN));
                        InvokeStatusChanged();
                        DoSaveImage(image, fileNameN, format, imageSettings);

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
            return false;
        });
        Success.ContinueWith(task =>
        {
            if (task.Result)
            {
                Log.Event(EventType.SaveImages, new EventParams
                {
                    Name = MiscResources.SaveImages,
                    Pages = images.Count,
                    FileFormat = Path.GetExtension(fileName)
                });
            }
        }, TaskContinuationOptions.OnlyOnRanToCompletion);

        return true;
    }

    private void DoSaveImage(ProcessedImage image, string path, ImageFormat format, ImageSettings imageSettings)
    {
        FileSystemHelper.EnsureParentDirExists(path);
        if (Equals(format, ImageFormat.Tiff))
        {
            _tiffHelper.SaveMultipage(new List<ProcessedImage> { image }, path, imageSettings.TiffCompression, (i, j) => { }, CancellationToken.None);
        }
        else if (Equals(format, ImageFormat.Jpeg))
        {
            var quality = imageSettings.JpegQuality.Clamp(0, 100);
            using var bitmap = image.Render();
            bitmap.Save(path, ImageFileFormat.Jpeg, quality);
        }
        else
        {
            using var bitmap = image.Render();
            var fileFormat = format switch
            {
                ImageFormat.Bitmap => ImageFileFormat.Bmp,
                ImageFormat.Jpeg => ImageFileFormat.Jpeg,
                ImageFormat.Png => ImageFileFormat.Png
            };
            bitmap.Save(path, fileFormat);
        }
    }

    private static ImageFormat GetImageFormat(string fileName)
    {
        string extension = Path.GetExtension(fileName);
        switch (extension.ToLower())
        {
            // TODO: Get rid of unsupported file types
            case ".bmp":
                return ImageFormat.Bitmap;
            // case ".emf":
            //     return ImageFormat.Emf;
            case ".gif":
                return ImageFormat.Gif;
            // case ".ico":
            //     return ImageFormat.Icon;
            case ".jpg":
            case ".jpeg":
                return ImageFormat.Jpeg;
            case ".png":
                return ImageFormat.Png;
            case ".tif":
            case ".tiff":
                return ImageFormat.Tiff;
            // case ".wmf":
            //     return ImageFormat.Wmf;
            default:
                return ImageFormat.Jpeg;
        }
    }
}