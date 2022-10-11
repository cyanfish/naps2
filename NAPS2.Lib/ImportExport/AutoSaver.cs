using NAPS2.EtoForms;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan;

namespace NAPS2.ImportExport;

public class AutoSaver
{
    private readonly ErrorOutput _errorOutput;
    private readonly DialogHelper _dialogHelper;
    private readonly OperationProgress _operationProgress;
    private readonly ISaveNotify _notify;
    private readonly IPdfExporter _pdfExporter;
    private readonly IOverwritePrompt _overwritePrompt;
    private readonly Naps2Config _config;
    private readonly ImageContext _imageContext;

    public AutoSaver(ErrorOutput errorOutput, DialogHelper dialogHelper,
        OperationProgress operationProgress, ISaveNotify notify, IPdfExporter pdfExporter,
        IOverwritePrompt overwritePrompt, Naps2Config config, ImageContext imageContext)
    {
        _errorOutput = errorOutput;
        _dialogHelper = dialogHelper;
        _operationProgress = operationProgress;
        _notify = notify;
        _pdfExporter = pdfExporter;
        _overwritePrompt = overwritePrompt;
        _config = config;
        _imageContext = imageContext;
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
                    // TODO: We should assume the returned sink may dispose what we give it, therefore we should make
                    // a clone before sending it out, and then dispose the clone when we're done with it
                    imageList.Add(img);
                    if (!settings.ClearImagesAfterSaving)
                    {
                        produceImage(img);
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
            foreach (var imageList in scans)
            {
                (bool success, string? filePath) =
                    await SaveOneFile(settings, placeholders, i++, imageList, scans.Count == 1);
                if (!success)
                {
                    ok = false;
                }
                if (success && firstFileSaved == null)
                {
                    firstFileSaved = filePath;
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
            if (_dialogHelper.PromptToSavePdfOrImage(subPath, out string? newPath))
            {
                subPath = placeholders.Substitute(newPath!, true, i);
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