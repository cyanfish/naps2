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

    public bool Start(string fileName, Placeholders placeholders, ICollection<ProcessedImage> images,
        PdfSettings pdfSettings, OcrParams ocrParams)
    {
        return Start(fileName, placeholders, images, pdfSettings, ocrParams, false, null);
    }

    public bool Start(string fileName, Placeholders placeholders, ICollection<ProcessedImage> images,
        PdfSettings pdfSettings, OcrParams ocrParams, bool email, EmailMessage? emailMessage)
    {
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

        RunAsync(async () =>
        {
            bool result = false;
            try
            {
                result = await _pdfExporter.Export(subFileName, images,
                    new PdfExportParams(pdfSettings.Metadata, pdfSettings.Encryption,
                        pdfSettings.Compat), ocrParams, OnProgress, CancelToken);
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
                // TODO: Here (and in every other operation that takes a list of images), clone the images on input and then dispose when finished
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
                    result = await _emailProviderFactory.Default.SendEmail(emailMessage, OnProgress, CancelToken);
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
}