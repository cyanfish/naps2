using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Ocr;
using NAPS2.Scan;
using NAPS2.WinForms;

namespace NAPS2.ImportExport;

public class AutoSaver
{
    private readonly IConfigProvider<PdfSettings> _pdfSettingsProvider;
    private readonly IConfigProvider<ImageSettings> _imageSettingsProvider;
    private readonly ErrorOutput _errorOutput;
    private readonly DialogHelper _dialogHelper;
    private readonly OperationProgress _operationProgress;
    private readonly ISaveNotify _notify;
    private readonly PdfExporter _pdfExporter;
    private readonly IOverwritePrompt _overwritePrompt;
    private readonly ScopedConfig _config;
    private readonly TiffHelper _tiffHelper;
    private readonly ImageContext _imageContext;

    public AutoSaver(IConfigProvider<PdfSettings> pdfSettingsProvider, IConfigProvider<ImageSettings> imageSettingsProvider, ErrorOutput errorOutput, DialogHelper dialogHelper, OperationProgress operationProgress, ISaveNotify notify, PdfExporter pdfExporter, IOverwritePrompt overwritePrompt, ScopedConfig config, TiffHelper tiffHelper, ImageContext imageContext)
    {
        _pdfSettingsProvider = pdfSettingsProvider;
        _imageSettingsProvider = imageSettingsProvider;
        _errorOutput = errorOutput;
        _dialogHelper = dialogHelper;
        _operationProgress = operationProgress;
        _notify = notify;
        _pdfExporter = pdfExporter;
        _overwritePrompt = overwritePrompt;
        _config = config;
        _tiffHelper = tiffHelper;
        _imageContext = imageContext;
    }

    public ScannedImageSource Save(AutoSaveSettings settings, ScannedImageSource source)
    {
        var sink = new ScannedImageSink();

        if (!settings.ClearImagesAfterSaving)
        {
            // Basic auto save, so keep track of images as we pipe them and try to auto save afterwards
            var imageList = new List<ProcessedImage>();
            source.ForEach(img =>
            {
                // TODO: We should assume the returned sink may dispose what we give it, therefore we should make
                // a clone before sending it out, and then dispose the clone when we're done with it
                // TODO: We should add tests for this class
                sink.PutImage(img);
                imageList.Add(img);
            }).ContinueWith(async t =>
            {
                try
                {
                    await InternalSave(settings, imageList);
                    if (t.IsFaulted && t.Exception != null)
                    {
                        sink.SetError(t.Exception.InnerException);
                    }
                }
                finally
                {
                    sink.SetCompleted();
                }
            });
            return sink.AsSource();
        }

        // Auto save without piping images
        source.ToList().ContinueWith(async t =>
        {
            if (await InternalSave(settings, t.Result))
            {
                foreach (ProcessedImage img in t.Result)
                {
                    img.Dispose();
                }
            }
            else
            {
                // Fallback in case auto save failed; pipe all the images back at once
                foreach (ProcessedImage img in t.Result)
                {
                    sink.PutImage(img);
                }
            }

            sink.SetCompleted();
        });
        return sink.AsSource();
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
                (bool success, string filePath) = await SaveOneFile(settings, placeholders, i++, imageList, scans.Count == 1);
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

    private async Task<(bool, string?)> SaveOneFile(AutoSaveSettings settings, Placeholders placeholders, int i, List<ProcessedImage> images, bool doNotify)
    {
        if (images.Count == 0)
        {
            return (true, null);
        }
        string subPath = placeholders.Substitute(settings.FilePath, true, i);
        if (settings.PromptForFilePath)
        {
            if (_dialogHelper.PromptToSavePdfOrImage(subPath, out string newPath))
            {
                subPath = placeholders.Substitute(newPath, true, i);
            }
        }
        var extension = Path.GetExtension(subPath);
        if (extension != null && extension.Equals(".pdf", StringComparison.InvariantCultureIgnoreCase))
        {
            if (File.Exists(subPath))
            {
                subPath = placeholders.Substitute(subPath, true, 0, 1);
            }
            var op = new SavePdfOperation(_pdfExporter, _overwritePrompt);
            if (op.Start(subPath, placeholders, images, _pdfSettingsProvider, _config.DefaultOcrParams()))
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
            var op = new SaveImagesOperation(_imageContext, _overwritePrompt, _tiffHelper);
            if (op.Start(subPath, placeholders, images, _imageSettingsProvider))
            {
                _operationProgress.ShowProgress(op);
            }
            bool success = await op.Success;
            if (success && doNotify)
            {
                _notify.ImagesSaved(images.Count, op.FirstFileSaved);
            }
            return (success, subPath);
        }
    }
}