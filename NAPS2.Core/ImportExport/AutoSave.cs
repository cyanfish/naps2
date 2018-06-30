using NAPS2.Config;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Lang.Resources;
using NAPS2.Ocr;
using NAPS2.Operation;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Util;
using NAPS2.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.ImportExport
{
    public class AutoSave : IAutoSave
    {
        private readonly IOperationFactory OperationFactory;
        private readonly IFormFactory formFactory;
        private readonly PdfSettingsContainer pdfSettingsContainer;
        private readonly OcrDependencyManager ocrDependencyManager;
        private readonly IErrorOutput errorOutput;
        private readonly AppConfigManager appConfigManager;
        private readonly FileNamePlaceholders fileNamePlaceholders;
        private readonly DialogHelper dialogHelper;

        public AutoSave(IOperationFactory OperationFactory, IFormFactory formFactory, PdfSettingsContainer pdfSettingsContainer, OcrDependencyManager ocrDependencyManager, IErrorOutput errorOutput, AppConfigManager appConfigManager, FileNamePlaceholders fileNamePlaceholders, DialogHelper dialogHelper)
        {
            this.OperationFactory = OperationFactory;
            this.formFactory = formFactory;
            this.pdfSettingsContainer = pdfSettingsContainer;
            this.ocrDependencyManager = ocrDependencyManager;
            this.errorOutput = errorOutput;
            this.appConfigManager = appConfigManager;
            this.fileNamePlaceholders = fileNamePlaceholders;
            this.dialogHelper = dialogHelper;
        }

        public bool Save(AutoSaveSettings settings, List<ScannedImage> images, ISaveNotify notify)
        {
            if (appConfigManager.Config.DisableAutoSave)
            {
                return false;
            }
            try
            {
                bool ok = true;
                DateTime now = DateTime.Now;
                int i = 0;
                string firstFileSaved = null;
                var scans = SaveSeparatorHelper.SeparateScans(new[] { images }, settings.Separator).ToList();
                foreach (var imageList in scans)
                {
                    ok &= SaveOneFile(settings, now, i++, imageList, scans.Count == 1 ? notify : null, ref firstFileSaved);
                }
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

        private bool SaveOneFile(AutoSaveSettings settings, DateTime now, int i, List<ScannedImage> images, ISaveNotify notify, ref string firstFileSaved)
        {
            if (images.Count == 0)
            {
                return true;
            }
            var form = formFactory.Create<FProgress>();
            string subPath = fileNamePlaceholders.SubstitutePlaceholders(settings.FilePath, now, true, i);
            if (settings.PromptForFilePath)
            {
                if (dialogHelper.PromptToSavePdfOrImage(subPath, out string newPath))
                {
                    subPath = fileNamePlaceholders.SubstitutePlaceholders(newPath, now, true, i);
                }
            }
            var extension = Path.GetExtension(subPath);
            if (extension?.Equals(".pdf", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                if (File.Exists(subPath))
                {
                    subPath = fileNamePlaceholders.SubstitutePlaceholders(subPath, now, true, 0, 1);
                }
                var op = OperationFactory.Create<SavePdfOperation>();
                form.Operation = op;
                if (op.Start(subPath, now, images, pdfSettingsContainer.PdfSettings, ocrDependencyManager.DefaultLanguageCode, false))
                {
                    form.ShowDialog();
                }
                if (op.Status.Success && firstFileSaved == null)
                {
                    firstFileSaved = subPath;
                }
                if (op.Status.Success)
                {
                    notify?.PdfSaved(subPath);
                }
                return op.Status.Success;
            }
            else
            {
                var op = OperationFactory.Create<SaveImagesOperation>();
                form.Operation = op;
                if (op.Start(subPath, now, images))
                {
                    form.ShowDialog();
                }
                if (op.Status.Success && firstFileSaved == null)
                {
                    firstFileSaved = op.FirstFileSaved;
                }
                if (op.Status.Success)
                {
                    notify?.ImagesSaved(images.Count, op.FirstFileSaved);
                }
                return op.Status.Success;
            }
        }
    }
}