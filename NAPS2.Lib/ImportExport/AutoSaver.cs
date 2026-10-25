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
    private readonly NameManager _nameManager;

    public AutoSaver(ErrorOutput errorOutput, DialogHelper dialogHelper,
        OperationProgress operationProgress, ISaveNotify notify, PdfExporter pdfExporter,
        IOverwritePrompt overwritePrompt, Naps2Config config, ImageContext imageContext, UiImageList imageList,
        NameManager nameManager)
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
        _nameManager = nameManager;
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
            string? firstFileSaved = null;
            var scans = SaveSeparatorHelper.SeparateScans(new[] { images }, settings.Separator)
                .Where(x => x.Count > 0).ToList();
            var saveParams = new SaveParams { OverwriteMode = OverwriteMode.Never, OpenEndedSequence = true };
            foreach (var (imagesToSave, i) in scans.Select((x, i) => (x, i)))
            {
                (bool success, string? filePath) = await SaveOneFile(settings, saveParams with { BatchIndex = i },
                    imagesToSave, doNotify: scans.Count == 1);
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

    private async Task<(bool, string?)> SaveOneFile(AutoSaveSettings settings, SaveParams saveParams,
        List<ProcessedImage> images, bool doNotify)
    {
        string path = settings.FilePath;
        if (settings.PromptForFilePath)
        {
            // The dialog handles conflicts itself, so the default is just the configured name with placeholders filled in
            var defaultPath = _nameManager.Get(path, new NameContext { DateTime = saveParams.DateTime });
            string? newPath = null!;
            if (Invoker.Current.InvokeGet(() => _dialogHelper.PromptToSavePdfOrImage(defaultPath, out newPath)))
            {
                path = newPath!;
                // The user chose (and confirmed overwriting) this exact name, so it shouldn't be numbered as part of
                // the sequence
                saveParams = saveParams with
                {
                    OverwriteMode = OverwriteMode.Confirmed,
                    OpenEndedSequence = false,
                    BatchIndex = null
                };
            }
            else
            {
                return (false, null);
            }
        }
        var extension = Path.GetExtension(_nameManager.Get(path, NameContext.FromSaveParams(saveParams)));
        if (extension.Equals(".pdf", StringComparison.InvariantCultureIgnoreCase))
        {
            var op = new SavePdfOperation(_pdfExporter, _overwritePrompt, _nameManager);
            if (op.Start(path, images, _config.Get(c => c.PdfSettings), _config.DefaultOcrParams(),
                    saveParams: saveParams))
            {
                _operationProgress.ShowProgress(op);
            }
            bool success = await op.Success;
            if (success && doNotify && op.FirstFileSaved != null)
            {
                _notify.PdfSaved(op.FirstFileSaved);
            }
            return (success, op.FirstFileSaved);
        }
        else
        {
            var op = new SaveImagesOperation(_overwritePrompt, _imageContext, _nameManager);
            if (op.Start(path, images, _config.Get(c => c.ImageSettings), saveParams))
            {
                _operationProgress.ShowProgress(op);
            }
            bool success = await op.Success;
            if (success && doNotify && op.FirstFileSaved != null)
            {
                _notify.ImagesSaved(images.Count, op.FirstFileSaved);
            }
            return (success, op.FirstFileSaved);
        }
    }
}
