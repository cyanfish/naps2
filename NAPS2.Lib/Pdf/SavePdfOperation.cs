using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.Ocr;

namespace NAPS2.Pdf;

internal class SavePdfOperation : OperationBase
{
    private readonly PdfExporter _pdfExporter;
    private readonly IOverwritePrompt _overwritePrompt;
    private readonly NameManager _nameManager;
    private readonly IEmailProviderFactory? _emailProviderFactory;

    public SavePdfOperation(PdfExporter pdfExporter, IOverwritePrompt overwritePrompt, NameManager nameManager,
        IEmailProviderFactory? emailProviderFactory = null)
    {
        _pdfExporter = pdfExporter;
        _overwritePrompt = overwritePrompt;
        _nameManager = nameManager;
        _emailProviderFactory = emailProviderFactory;

        AllowCancel = true;
        AllowBackground = true;
    }

    public string? FirstFileSaved { get; private set; }

    /// <summary>
    /// Saves the provided images to a PDF file with the given name, or one PDF per image if specified by the settings.
    /// </summary>
    /// <param name="fileName">The file name to save to, which may contain placeholders.</param>
    /// <param name="images">The images to save.</param>
    /// <param name="pdfSettings">The PDF settings.</param>
    /// <param name="ocrParams">The OCR parameters.</param>
    /// <param name="emailMessage">If specified, the saved file(s) are attached to the message and it is sent.</param>
    /// <param name="saveParams">How files are named and existing files are handled.</param>
    public bool Start(string fileName, ICollection<ProcessedImage> images, PdfSettings pdfSettings,
        OcrParams ocrParams, EmailMessage? emailMessage = null, SaveParams? saveParams = null)
    {
        ProgressTitle = emailMessage != null ? MiscResources.EmailPdfProgress : MiscResources.SavePdfProgress;
        saveParams ??= new SaveParams();

        var subFileName = _nameManager.Get(fileName, NameContext.FromSaveParams(saveParams));
        if (Directory.Exists(subFileName))
        {
            // Not supposed to be a directory, but ok...
            fileName = Path.Combine(subFileName, "$(n).pdf");
        }
        var imagesByFile = pdfSettings.SinglePagePdfs
            ? images.Select(x => new[] { x }).ToArray()
            : new[] { images.ToArray() };
        saveParams = saveParams with { BatchCount = Math.Max(saveParams.BatchCount ?? 1, imagesByFile.Length) };
        var context = NameContext.FromSaveParams(saveParams);

        var reservations = Enumerable.Range(0, imagesByFile.Length)
            .Select(i => _nameManager.Reserve(fileName, context with { BatchIndex = (context.BatchIndex ?? 0) + i }))
            .ToDisposableList();
        var fileNames = reservations.Select(x => x.Name).ToArray();

        Status = new OperationStatus
        {
            StatusText = string.Format(MiscResources.SavingFormat, Path.GetFileName(fileNames[0])),
            MaxProgress = images.Count
        };

        var singleFile = imagesByFile.Length == 1;
        if (singleFile && saveParams.OverwriteMode.ShouldPromptForPath(fileNames[0]) &&
            _overwritePrompt.ConfirmOverwrite(fileNames[0]) != OverwriteResponse.Yes)
        {
            reservations.Dispose();
            FailedToStart();
            return false;
        }

        RunAsync(async () =>
        {
            bool result = false;
            try
            {
                int i = 0;
                foreach (var imagesForFile in imagesByFile)
                {
                    if (CancelToken.IsCancellationRequested)
                    {
                        result = false;
                        break;
                    }
                    var currentFileName = fileNames[i];
                    Status.StatusText = string.Format(MiscResources.SavingFormat, Path.GetFileName(currentFileName));
                    InvokeStatusChanged();

                    var progress = new ProgressHandler(singleFile ? OnProgress : null, CancelToken);
                    result = await RunWithRetry(async () =>
                            await WaitForFileNotInUse(currentFileName) &&
                            await _pdfExporter.Export(currentFileName, imagesForFile,
                                new PdfExportParams(pdfSettings.Metadata.Substitute(_nameManager, context.DateTime),
                                    pdfSettings.Encryption, pdfSettings.Compat), ocrParams, progress),
                        GetSaveErrorMessage);
                    if (!result)
                    {
                        break;
                    }
                    emailMessage?.Attachments.Add(new EmailAttachment
                    {
                        FilePath = currentFileName,
                        AttachmentName = Path.GetFileName(currentFileName)
                    });
                    if (i == 0)
                    {
                        FirstFileSaved = currentFileName;
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
            catch (Exception ex)
            {
                Log.ErrorException(MiscResources.ErrorSaving, ex);
                InvokeError(MiscResources.ErrorSaving, ex);
            }
            finally
            {
                reservations.Dispose();
                GC.Collect();
            }

            if (result && emailMessage != null && _emailProviderFactory != null)
            {
                if (CancelToken.IsCancellationRequested)
                {
                    return false;
                }
                Status.StatusText = MiscResources.UploadingEmail;
                Status.CurrentProgress = 0;
                Status.MaxProgress = 1;
                Status.ProgressType = OperationProgressType.MB;
                InvokeStatusChanged();

                result = await RunWithRetry(
                    () => _emailProviderFactory.Default.SendEmail(emailMessage, ProgressHandler),
                    _ => MiscResources.ErrorEmailing);
            }

            return result;
        });
        Success.ContinueWith(task =>
        {
            if (task.Result)
            {
                if (emailMessage != null)
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
}
