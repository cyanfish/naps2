using NAPS2.ImportExport.Email;
using NAPS2.Ocr;

namespace NAPS2.ImportExport.Pdf;

public class SavePdfOperation : OperationBase
{
    private readonly IPdfExporter _pdfExporter;
    private readonly IOverwritePrompt _overwritePrompt;
    private readonly IEmailProviderFactory? _emailProviderFactory;

    public SavePdfOperation(IPdfExporter pdfExporter, IOverwritePrompt overwritePrompt,
        IEmailProviderFactory? emailProviderFactory = null)
    {
        _pdfExporter = pdfExporter;
        _overwritePrompt = overwritePrompt;
        _emailProviderFactory = emailProviderFactory;

        AllowCancel = true;
        AllowBackground = true;
    }

    // TODO: Do something with this re: notifications?
    public string? FirstFileSaved { get; private set; }

    public bool Start(string fileName, Placeholders placeholders, ICollection<ProcessedImage> images,
        PdfSettings pdfSettings, OcrParams ocrParams)
    {
        return Start(fileName, placeholders, images, pdfSettings, ocrParams, false, null);
    }

    public bool Start(string fileName, Placeholders placeholders, ICollection<ProcessedImage> images,
        PdfSettings pdfSettings, OcrParams ocrParams, bool email, EmailMessage? emailMessage)
    {
        // TODO: This needs tests. And ideally simplification.
        ProgressTitle = email ? MiscResources.EmailPdfProgress : MiscResources.SavePdfProgress;
        var subFileName = placeholders.Substitute(fileName);
        Status = new OperationStatus
        {
            StatusText = string.Format(MiscResources.SavingFormat, Path.GetFileName(subFileName)),
            MaxProgress = images.Count
        };

        if (Directory.Exists(subFileName))
        {
            // Not supposed to be a directory, but ok...
            subFileName = placeholders.Substitute(Path.Combine(subFileName, "$(n).pdf"));
        }
        // TODO: We need to make sure we dispose of the images in all cases (e.g. on choosing not to overwrite)
        var singleFile = !pdfSettings.SinglePagePdfs || images.Count == 1;
        if (singleFile)
        {
            if (File.Exists(subFileName))
            {
                // TODO: Gtk auto prompts for overwrite in the save dialog. How to handle this and avoid duplicate prompts?
                // Can we just change the overwrite prompt implementation or will that be a problem in some cases where the save dialog isn't used, e.g. auto save, batch, etc?
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
        }

        var imagesByFile = pdfSettings.SinglePagePdfs ? images.Select(x => new[] { x }).ToArray() : new[] { images.ToArray() };
        RunAsync(async () =>
        {
            bool result = false;
            try
            {
                int digits = (int)Math.Floor(Math.Log10(images.Count)) + 1;
                int i = 0;
                foreach (var imagesForFile in imagesByFile)
                {
                    var currentFileName = placeholders.Substitute(fileName, true, i, singleFile ? 0 : digits);
                    Status.StatusText = string.Format(MiscResources.SavingFormat, Path.GetFileName(currentFileName));
                    InvokeStatusChanged();
                    if (singleFile && IsFileInUse(currentFileName, out var ex))
                    {
                        InvokeError(MiscResources.FileInUse, ex!);
                        break;
                    }

                    var progress = new ProgressHandler(singleFile ? OnProgress : null, CancelToken);
                    result = await _pdfExporter.Export(currentFileName, imagesForFile,
                        new PdfExportParams(pdfSettings.Metadata, pdfSettings.Encryption,
                            pdfSettings.Compat), ocrParams, progress);
                    if (!result || CancelToken.IsCancellationRequested)
                    {
                        break;
                    }
                    emailMessage?.Attachments.Add(new EmailAttachment(currentFileName, Path.GetFileName(currentFileName)));
                    if (i == 0)
                    {
                        FirstFileSaved = subFileName;
                    }
                    i++;
                    if (!singleFile)
                    {
                        OnProgress(i, imagesByFile.Length);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                InvokeError(MiscResources.DontHavePermission, ex);
            }
            catch (IOException ex)
            {
                if (File.Exists(subFileName))
                {
                    InvokeError(MiscResources.FileInUse, ex);
                }
                else
                {
                    Log.ErrorException(MiscResources.ErrorSaving, ex);
                    InvokeError(MiscResources.ErrorSaving, ex);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException(MiscResources.ErrorSaving, ex);
                InvokeError(MiscResources.ErrorSaving, ex);
            }
            finally
            {
                foreach (var image in images)
                {
                    image.Dispose();
                }
                GC.Collect();
            }

            if (result && email && emailMessage != null && _emailProviderFactory != null)
            {
                Status.StatusText = MiscResources.UploadingEmail;
                Status.CurrentProgress = 0;
                Status.MaxProgress = 1;
                Status.ProgressType = OperationProgressType.MB;
                InvokeStatusChanged();

                try
                {
                    result = await _emailProviderFactory.Default.SendEmail(emailMessage, ProgressHandler);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    Log.ErrorException(MiscResources.ErrorEmailing, ex);
                    InvokeError(MiscResources.ErrorEmailing, ex);
                }
            }

            return result;
        });
        Success.ContinueWith(task =>
        {
            if (task.Result)
            {
                if (email)
                {
                    Log.Event(EventType.Email, new EventParams
                    {
                        Name = MiscResources.EmailPdf,
                        Pages = images.Count,
                        FileFormat = ".pdf"
                    });
                }
                else
                {
                    Log.Event(EventType.SavePdf, new EventParams
                    {
                        Name = MiscResources.SavePdf,
                        Pages = images.Count,
                        FileFormat = ".pdf"
                    });
                }
            }
        }, TaskContinuationOptions.OnlyOnRanToCompletion);

        return true;
    }

    private bool IsFileInUse(string filePath, out Exception? exception)
    {
        // TODO: Generalize this for images too
        exception = null;
        if (File.Exists(filePath))
        {
            try
            {
                using (new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                }
            }
            catch (IOException ex)
            {
                exception = ex;
                return true;
            }
        }
        return false;
    }
}