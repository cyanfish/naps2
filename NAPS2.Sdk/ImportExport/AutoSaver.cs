using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Config;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Ocr;
using NAPS2.Operation;
using NAPS2.Scan;
using NAPS2.Images;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.ImportExport;

public class AutoSaver
{
    private readonly IConfigProvider<PdfSettings> _pdfSettingsProvider;
    private readonly IConfigProvider<ImageSettings> _imageSettingsProvider;
    private readonly OcrEngineManager _ocrEngineManager;
    private readonly OcrRequestQueue _ocrRequestQueue;
    private readonly ErrorOutput _errorOutput;
    private readonly DialogHelper _dialogHelper;
    private readonly OperationProgress _operationProgress;
    private readonly ISaveNotify? _notify;
    private readonly PdfExporter _pdfExporter;
    private readonly OverwritePrompt _overwritePrompt;
    private readonly BitmapRenderer _bitmapRenderer;
    private readonly ScopedConfig _config;

    public AutoSaver(IConfigProvider<PdfSettings> pdfSettingsProvider, IConfigProvider<ImageSettings> imageSettingsProvider, OcrEngineManager ocrEngineManager, OcrRequestQueue ocrRequestQueue, ErrorOutput errorOutput, DialogHelper dialogHelper, OperationProgress operationProgress, ISaveNotify notify, PdfExporter pdfExporter, OverwritePrompt overwritePrompt, BitmapRenderer bitmapRenderer, ScopedConfig config)
    {
        _pdfSettingsProvider = pdfSettingsProvider;
        _imageSettingsProvider = imageSettingsProvider;
        _ocrEngineManager = ocrEngineManager;
        _ocrRequestQueue = ocrRequestQueue;
        _errorOutput = errorOutput;
        _dialogHelper = dialogHelper;
        _operationProgress = operationProgress;
        _notify = notify;
        _pdfExporter = pdfExporter;
        _overwritePrompt = overwritePrompt;
        _bitmapRenderer = bitmapRenderer;
        _config = config;
    }

    public ScannedImageSource Save(AutoSaveSettings settings, ScannedImageSource source)
    {
        var sink = new ScannedImageSink();

        if (!settings.ClearImagesAfterSaving)
        {
            // Basic auto save, so keep track of images as we pipe them and try to auto save afterwards
            var imageList = new List<ScannedImage>();
            source.ForEach(img =>
            {
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
                foreach (ScannedImage img in t.Result)
                {
                    img.Dispose();
                }
            }
            else
            {
                // Fallback in case auto save failed; pipe all the images back at once
                foreach (ScannedImage img in t.Result)
                {
                    sink.PutImage(img);
                }
            }

            sink.SetCompleted();
        });
        return sink.AsSource();
    }

    private async Task<bool> InternalSave(AutoSaveSettings settings, List<ScannedImage> images)
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
            if (_notify != null && scans.Count > 1 && ok)
            {
                // Can't just do images.Count because that includes patch codes
                int imageCount = scans.SelectMany(x => x).Count();
                _notify.ImagesSaved(imageCount, firstFileSaved);
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
        
    private async Task<(bool, string)> SaveOneFile(AutoSaveSettings settings, Placeholders placeholders, int i, List<ScannedImage> images, bool doNotify)
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
            var ocrContext = new OcrContext(_config.DefaultOcrParams(), _ocrEngineManager, _ocrRequestQueue);
            if (op.Start(subPath, placeholders, images, _pdfSettingsProvider, ocrContext))
            {
                _operationProgress.ShowProgress(op);
            }
            bool success = await op.Success;
            if (success && doNotify)
            {
                _notify?.PdfSaved(subPath);
            }
            return (success, subPath);
        }
        else
        {
            var op = new SaveImagesOperation(_overwritePrompt, _bitmapRenderer, new TiffHelper(_bitmapRenderer));
            if (op.Start(subPath, placeholders, images, _imageSettingsProvider))
            {
                _operationProgress.ShowProgress(op);
            }
            bool success = await op.Success;
            if (success && doNotify)
            {
                _notify?.ImagesSaved(images.Count, op.FirstFileSaved);
            }
            return (success, subPath);
        }
    }
}