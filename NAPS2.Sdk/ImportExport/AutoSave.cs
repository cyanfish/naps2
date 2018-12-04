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

namespace NAPS2.ImportExport
{
    public class AutoSave : IAutoSave
    {
        private readonly IOperationFactory operationFactory;
        private readonly PdfSettingsContainer pdfSettingsContainer;
        private readonly OcrManager ocrManager;
        private readonly IErrorOutput errorOutput;
        private readonly DialogHelper dialogHelper;
        private readonly OperationProgress operationProgress;

        public AutoSave(IOperationFactory operationFactory, PdfSettingsContainer pdfSettingsContainer, OcrManager ocrManager, IErrorOutput errorOutput, DialogHelper dialogHelper, OperationProgress operationProgress)
        {
            this.operationFactory = operationFactory;
            this.pdfSettingsContainer = pdfSettingsContainer;
            this.ocrManager = ocrManager;
            this.errorOutput = errorOutput;
            this.dialogHelper = dialogHelper;
            this.operationProgress = operationProgress;
        }

        public async Task<bool> Save(AutoSaveSettings settings, List<ScannedImage> images, ISaveNotify notify)
        {
            if (AppConfig.Current.DisableAutoSave)
            {
                return false;
            }
            try
            {
                bool ok = true;
                var placeholders = Placeholders.All.WithDate(DateTime.Now);
                int i = 0;
                string firstFileSaved = null;
                var scans = SaveSeparatorHelper.SeparateScans(new[] { images }, settings.Separator).ToList();
                foreach (var imageList in scans)
                {
                    (bool success, string filePath) = await SaveOneFile(settings, placeholders, i++, imageList, scans.Count == 1 ? notify : null);
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
                if (notify != null && scans.Count > 1 && ok)
                {
                    // Can't just do images.Count because that includes patch codes
                    int imageCount = scans.SelectMany(x => x).Count();
                    notify.ImagesSaved(imageCount, firstFileSaved);
                }
                return ok;
            }
            catch (Exception ex)
            {
                Log.ErrorException(MiscResources.AutoSaveError, ex);
                errorOutput.DisplayError(MiscResources.AutoSaveError, ex);
                return false;
            }
        }
        
        private async Task<(bool, string)> SaveOneFile(AutoSaveSettings settings, Placeholders placeholders, int i, List<ScannedImage> images, ISaveNotify notify)
        {
            if (images.Count == 0)
            {
                return (true, null);
            }
            string subPath = placeholders.Substitute(settings.FilePath, true, i);
            if (settings.PromptForFilePath)
            {
                if (dialogHelper.PromptToSavePdfOrImage(subPath, out string newPath))
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
                var op = operationFactory.Create<SavePdfOperation>();
                if (op.Start(subPath, placeholders, images, pdfSettingsContainer.PdfSettings, ocrManager.DefaultParams, false, null))
                {
                    operationProgress.ShowProgress(op);
                }
                bool success = await op.Success;
                if (success)
                {
                    notify?.PdfSaved(subPath);
                }
                return (success, subPath);
            }
            else
            {
                var op = operationFactory.Create<SaveImagesOperation>();
                if (op.Start(subPath, placeholders, images))
                {
                    operationProgress.ShowProgress(op);
                }
                bool success = await op.Success;
                if (success)
                {
                    notify?.ImagesSaved(images.Count, op.FirstFileSaved);
                }
                return (success, subPath);
            }
        }
    }
}