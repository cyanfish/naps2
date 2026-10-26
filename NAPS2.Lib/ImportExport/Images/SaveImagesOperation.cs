namespace NAPS2.ImportExport.Images;

internal class SaveImagesOperation : OperationBase
{
    private readonly IOverwritePrompt _overwritePrompt;
    private readonly ImageContext _imageContext;
    private readonly NameManager _nameManager;

    public SaveImagesOperation(IOverwritePrompt overwritePrompt, ImageContext imageContext, NameManager nameManager)
    {
        _overwritePrompt = overwritePrompt;
        _imageContext = imageContext;
        _nameManager = nameManager;

        ProgressTitle = MiscResources.SaveImagesProgress;
        AllowCancel = true;
        AllowBackground = true;
    }

    public string? FirstFileSaved { get; private set; }

    /// <summary>
    /// Saves the provided collection of images to a file with the given name. The image type is inferred from the file extension.
    /// If multiple images are provided, they will be saved to files with numeric identifiers, e.g. img1.jpg, img2.jpg, etc..
    /// </summary>
    /// <param name="fileName">The file name to save to, which may contain placeholders. For multiple images, a number
    /// is inserted before the extension if there is no number placeholder.</param>
    /// <param name="images">The collection of images to save.</param>
    /// <param name="imageSettings">The image settings.</param>
    /// <param name="saveParams">How files are named and existing files are handled.</param>
    public bool Start(string fileName, IList<ProcessedImage> images, ImageSettings imageSettings,
        SaveParams? saveParams = null)
    {
        Status = new OperationStatus
        {
            MaxProgress = images.Count
        };
        saveParams ??= new SaveParams();
        var context = NameContext.FromSaveParams(saveParams);

        var subFileName = _nameManager.Get(fileName, context);
        if (Directory.Exists(subFileName))
        {
            // Not supposed to be a directory, but ok...
            fileName = Path.Combine(subFileName, "$(n).jpg");
            subFileName = _nameManager.Get(fileName, context);
        }
        var format = ImageContext.GetFileFormatFromExtension(subFileName);
        int fileCount = GetFileCount(format, images.Count, imageSettings);
        saveParams = saveParams with { BatchCount = Math.Max(saveParams.BatchCount ?? 1, fileCount) };
        context = NameContext.FromSaveParams(saveParams);

        var reservations = Enumerable.Range(0, fileCount)
            .Select(i => _nameManager.Reserve(fileName, context with { BatchIndex = (context.BatchIndex ?? 0) + i }))
            .ToDisposableList();
        var fileNames = reservations.Select(x => x.Name).ToArray();

        RunAsync(async () =>
        {
            try
            {
                if (format == ImageFileFormat.Unknown)
                {
                    throw new ArgumentException($"Could not infer file format from extension: {subFileName}");
                }

                if (IsMultiPageTiff(format, imageSettings))
                {
                    var tiffFileName = fileNames[0];
                    if (saveParams.OverwriteMode.ShouldPromptForPath(tiffFileName) &&
                        _overwritePrompt.ConfirmOverwrite(tiffFileName) != OverwriteResponse.Yes)
                    {
                        return false;
                    }
                    Status.StatusText = string.Format(MiscResources.SavingFormat, Path.GetFileName(tiffFileName));
                    FirstFileSaved = tiffFileName;
                    return await RunWithRetry(async () =>
                    {
                        if (!await WaitForFileNotInUse(tiffFileName))
                        {
                            return false;
                        }
                        FileSystemHelper.EnsureParentDirExists(tiffFileName);
                        using var renderedImages = images.Select(x => x.Render()).ToDisposableList();
                        return _imageContext.TiffWriter.SaveTiff(renderedImages.InnerList, tiffFileName,
                            imageSettings.TiffCompression.ToTiffCompressionType(), ProgressHandler);
                    }, GetSaveErrorMessage);
                }

                int i = 0;
                foreach (ProcessedImage image in images)
                {
                    if (CancelToken.IsCancellationRequested)
                    {
                        return false;
                    }
                    Status.CurrentProgress = i;
                    InvokeStatusChanged();

                    var currentFileName = fileNames[i];
                    if (saveParams.OverwriteMode.ShouldPromptForPath(currentFileName))
                    {
                        var overwriteResponse = _overwritePrompt.ConfirmOverwrite(currentFileName);
                        if (overwriteResponse == OverwriteResponse.No)
                        {
                            continue;
                        }
                        if (overwriteResponse == OverwriteResponse.Abort)
                        {
                            return false;
                        }
                    }
                    Status.StatusText = string.Format(MiscResources.SavingFormat, Path.GetFileName(currentFileName));
                    InvokeStatusChanged();
                    if (!await SaveImageWithRetry(image, currentFileName, format, imageSettings))
                    {
                        return false;
                    }
                    FirstFileSaved ??= currentFileName;
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
                reservations.Dispose();
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

    /// <summary>
    /// Gets the number of files that saving the given number of images to the given file name will produce.
    /// </summary>
    public static int GetFileCount(string fileName, int imageCount, ImageSettings imageSettings) =>
        GetFileCount(ImageContext.GetFileFormatFromExtension(fileName), imageCount, imageSettings);

    private static int GetFileCount(ImageFileFormat format, int imageCount, ImageSettings imageSettings) =>
        IsMultiPageTiff(format, imageSettings) ? 1 : imageCount;

    private static bool IsMultiPageTiff(ImageFileFormat format, ImageSettings imageSettings) =>
        format == ImageFileFormat.Tiff && !imageSettings.SinglePageTiff;

    private Task<bool> SaveImageWithRetry(ProcessedImage image, string path, ImageFileFormat format,
        ImageSettings imageSettings)
    {
        return RunWithRetry(async () =>
        {
            // Check if the file is in use before saving (including on retries) to avoid unnecessary work
            if (!await WaitForFileNotInUse(path))
            {
                return false;
            }
            DoSaveImage(image, path, format, imageSettings);
            return true;
        }, GetSaveErrorMessage);
    }

    private void DoSaveImage(ProcessedImage image, string path, ImageFileFormat format, ImageSettings imageSettings)
    {
        FileSystemHelper.EnsureParentDirExists(path);
        using var renderedImage = image.Render();
        if (format == ImageFileFormat.Tiff)
        {
            _imageContext.TiffWriter.SaveTiff(new[] { renderedImage }, path,
                imageSettings.TiffCompression.ToTiffCompressionType(), CancelToken);
        }
        else
        {
            // Quality will be ignored when not needed
            // TODO: Scale quality differently for jpeg2000?
            var quality = imageSettings.JpegQuality.Clamp(0, 100);
            renderedImage.Save(path, format, new ImageSaveOptions { Quality = quality });
        }
    }
}
