using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Config;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.ImportExport
{
    public interface IAutoSave
    {
        bool AutoSave(AutoSaveSettings settings, List<ScannedImage> images);
    }

    public class WinFormsAutoSave : IAutoSave
    {
        private readonly IOperationFactory operationFactory;
        private readonly IFormFactory formFactory;
        private readonly PdfSettingsContainer pdfSettingsContainer;
        private readonly IUserConfigManager userConfigManager;
        private readonly IErrorOutput errorOutput;
        private readonly AppConfigManager appConfigManager;
        private readonly FileNamePlaceholders fileNamePlaceholders;

        public WinFormsAutoSave(IOperationFactory operationFactory, IFormFactory formFactory, PdfSettingsContainer pdfSettingsContainer, IUserConfigManager userConfigManager, IErrorOutput errorOutput, AppConfigManager appConfigManager, FileNamePlaceholders fileNamePlaceholders)
        {
            this.operationFactory = operationFactory;
            this.formFactory = formFactory;
            this.pdfSettingsContainer = pdfSettingsContainer;
            this.userConfigManager = userConfigManager;
            this.errorOutput = errorOutput;
            this.appConfigManager = appConfigManager;
            this.fileNamePlaceholders = fileNamePlaceholders;
        }

        public bool AutoSave(AutoSaveSettings settings, List<ScannedImage> images)
        {
            if (appConfigManager.Config.DisableAutoSave)
            {
                return false;
            }
            try
            {
                bool ok = true;
                DateTime now = DateTime.Now;
                if (settings.Separator == AutoSaveSeparator.FilePerScan)
                {
                    if (!SaveOneFile(settings, now, 0, images))
                    {
                        ok = false;
                    }
                }
                else if (settings.Separator == AutoSaveSeparator.FilePerPage)
                {
                    for (int i = 0; i < images.Count; i++)
                    {
                        if (!SaveOneFile(settings, now, i, new List<ScannedImage> { images[i] }))
                        {
                            ok = false;
                        }
                    }
                }
                else if (settings.Separator == AutoSaveSeparator.PatchT)
                {
                    var imageSet = new List<ScannedImage>();
                    int fileIndex = 0;
                    foreach (ScannedImage img in images)
                    {
                        if (img.PatchCode == PatchCode.PatchT)
                        {
                            if (imageSet.Count > 0)
                            {
                                if (!SaveOneFile(settings, now, fileIndex++, imageSet))
                                {
                                    ok = false;
                                }
                                imageSet.Clear();
                            }
                        }
                        else
                        {
                            imageSet.Add(img);
                        }
                    }
                    if (!SaveOneFile(settings, now, fileIndex, imageSet))
                    {
                        ok = false;
                    }
                }
                return ok;
            }
            catch (Exception ex)
            {
                Log.ErrorException(MiscResources.AutoSaveError, ex);
                errorOutput.DisplayError(MiscResources.AutoSaveError);
                return false;
            }
        }

        private bool SaveOneFile(AutoSaveSettings settings, DateTime now, int i, List<ScannedImage> images)
        {
            if (images.Count == 0)
            {
                return true;
            }
            var form = formFactory.Create<FProgress>();
            var subPath = fileNamePlaceholders.SubstitutePlaceholders(settings.FilePath, now, true, i);
            var extension = Path.GetExtension(subPath);
            if (extension != null && extension.Equals(".pdf", StringComparison.InvariantCultureIgnoreCase))
            {
                if (File.Exists(subPath))
                {
                    subPath = fileNamePlaceholders.SubstitutePlaceholders(subPath, now, true, 0, 1);
                }
                var op = operationFactory.Create<SavePdfOperation>();
                form.Operation = op;
                var ocrLanguageCode = userConfigManager.Config.EnableOcr ? userConfigManager.Config.OcrLanguageCode : null;
                if (op.Start(subPath, now, images, pdfSettingsContainer.PdfSettings, ocrLanguageCode, false))
                {
                    form.ShowDialog();
                }
                return op.Status.Success;
            }
            else
            {
                var op = operationFactory.Create<SaveImagesOperation>();
                form.Operation = op;
                if (op.Start(subPath, now, images))
                {
                    form.ShowDialog();
                }
                return op.Status.Success;
            }
        }
    }

    public class ConsoleAutoSave : IAutoSave
    {
        public bool AutoSave(AutoSaveSettings settings, List<ScannedImage> images)
        {
            // Not supported in NAPS2.Console
            return false;
        }
    }
}
