using NAPS2.EtoForms;
using NAPS2.EtoForms.Notifications;
using NAPS2.ImportExport.Images;
using NAPS2.Pdf;
using NAPS2.Scan;

namespace NAPS2.ImportExport;

public class AutoSaver
{
    private readonly ErrorOutput _errorOutput;
    private readonly DialogHelper _dialogHelper;
    private readonly OperationProgress _operationProgress;
    private readonly ISaveNotify _notify;
    private readonly PdfExporter _pdfExporter;
    private readonly IOverwritePrompt _overwritePrompt;
    private readonly Naps2Config _config;
    private readonly ImageContext _imageContext;
    private readonly UiImageList _imageList;

    public AutoSaver(ErrorOutput errorOutput, DialogHelper dialogHelper,
        OperationProgress operationProgress, ISaveNotify notify, PdfExporter pdfExporter,
        IOverwritePrompt overwritePrompt, Naps2Config config, ImageContext imageContext, UiImageList imageList)
    {
        _errorOutput = errorOutput;
        _dialogHelper = dialogHelper;
        _operationProgress = operationProgress;
        _notify = notify;
        _pdfExporter = pdfExporter;
        _overwritePrompt = overwritePrompt;
        _config = config;
        _imageContext = imageContext;
        _imageList = imageList;
    }

    public IAsyncEnumerable<ProcessedImage> Save(AutoSaveSettings settings, IAsyncEnumerable<ProcessedImage> images)
    {
        return AsyncProducers.RunProducer<ProcessedImage>(async produceImage =>
        {
            var imageList = new List<ProcessedImage>();
            try
            {
                await foreach (var img in images)
                {
                    imageList.Add(img);
                    if (!settings.ClearImagesAfterSaving)
                    {
                        produceImage(img.Clone());
                    }
                }
            }
            finally
            {
                if (!await InternalSave(settings, imageList) && settings.ClearImagesAfterSaving)
                {
                    // Fallback in case auto save failed; pipe all the images back at once
                    foreach (var img in imageList)
                    {
                        produceImage(img);
                    }
                }
                else
                {
                    foreach (var img in imageList)
                    {
                        img.Dispose();
                    }
                }
            }
        });
    }

    private async Task<bool> InternalSave(AutoSaveSettings settings, List<ProcessedImage> images)
    {
        try
        {
            bool ok = true;
            var placeholders = Placeholders.All.WithDate(DateTime.Now);
            int i = 0;
            string? firstFileSaved = null;
            var scans = SaveSeparatorHelper.SeparateScans(new[] { images }, settings.Separator).ToList();
            foreach (var imagesToSave in scans)
            {
                (bool success, string? filePath) =
                    await SaveOneFile(settings, placeholders, i++, imagesToSave, scans.Count == 1);
                if (success)
                {
                    // Normally we're supposed to take the CurrentState before the save operation starts, but that
                    // doesn't really work here since populating the UiImageList happens asynchronously so the images
                    // we're saving might not be present yet. In practice waiting until after saving will ensure the
                    // list is populated so that this logic works correctly.
                    _imageList.MarkSaved(_imageList.CurrentState, imagesToSave);
                    firstFileSaved ??= filePath;
                }
                else
                {
                    ok = false;
                }
            }
            // TODO: Shouldn't this give duplicate notifications?
            if (scans.Count > 1 && ok)
            {
                // Can't just do images.Count because that includes patch codes
                int imageCount = scans.SelectMany(x => x).Count();
                _notify.ImagesSaved(imageCount, firstFileSaved!);
            }
            return ok;
        }
        catch (Exception ex)
        {
            Log.ErrorException(MiscResources.AutoSaveError, ex);
            _errorOutput.DisplayError(MiscResources.AutoSaveError, ex);
            return false;
        }
    }

    private async Task<(bool, string?)> SaveOneFile(AutoSaveSettings settings, Placeholders placeholders, int i,
        List<ProcessedImage> images, bool doNotify)
    {
        if (images.Count == 0)
        {
            return (true, null);
        }
        string subPath = placeholders.Substitute(settings.FilePath, true, i);
        if (settings.PromptForFilePath)
        {
            string? newPath = null!;
            if (Invoker.Current.InvokeGet(() => _dialogHelper.PromptToSavePdfOrImage(subPath, out newPath)))
            {
                subPath = placeholders.Substitute(newPath!, true, i);
            }
            else
            {
                return (false, null);
            }
        }
        // TODO: This placeholder handling is complex and wrong in some cases (e.g. FilePerScan with ext = "jpg")
        // TODO: Maybe have initial placeholders that replace date, then rely on the ops to increment the file num
        var extension = Path.GetExtension(subPath);
        if (extension != null && extension.Equals(".pdf", StringComparison.InvariantCultureIgnoreCase))
        {
            if (File.Exists(subPath))
            {
                subPath = placeholders.Substitute(subPath, true, 0, 1);
            }
            var op = new SavePdfOperation(_pdfExporter, _overwritePrompt);
            if (op.Start(subPath, placeholders, images, _config.Get(c => c.PdfSettings), _config.DefaultOcrParams()))
            {
                _operationProgress.ShowProgress(op);
            }
            bool success = await op.Success;
            if (success && doNotify)
            {
                _notify.PdfSaved(subPath);
            }
            return (success, subPath);
        }
        else
        {
            var op = new SaveImagesOperation(_overwritePrompt, _imageContext);
            if (op.Start(subPath, placeholders, images, _config.Get(c => c.ImageSettings)))
            {
                _operationProgress.ShowProgress(op);
            }
            bool success = await op.Success;
            if (success && doNotify && op.FirstFileSaved != null)
            {
                _notify.ImagesSaved(images.Count, op.FirstFileSaved);
            }
            return (success, subPath);
        }
    }
}