using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NAPS2.Config;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Lang.Resources;
using NAPS2.Ocr;
using NAPS2.Operation;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public class WinFormsExportHelper
    {
        private readonly PdfSettingsContainer pdfSettingsContainer;
        private readonly ImageSettingsContainer imageSettingsContainer;
        private readonly EmailSettingsContainer emailSettingsContainer;
        private readonly DialogHelper dialogHelper;
        private readonly FileNamePlaceholders fileNamePlaceholders;
        private readonly ChangeTracker changeTracker;
        private readonly IOperationFactory operationFactory;
        private readonly IFormFactory formFactory;
        private readonly OcrDependencyManager ocrDependencyManager;
        private readonly IEmailer emailer;

        public WinFormsExportHelper(PdfSettingsContainer pdfSettingsContainer, ImageSettingsContainer imageSettingsContainer, EmailSettingsContainer emailSettingsContainer, DialogHelper dialogHelper, FileNamePlaceholders fileNamePlaceholders, ChangeTracker changeTracker, IOperationFactory operationFactory, IFormFactory formFactory, OcrDependencyManager ocrDependencyManager, IEmailer emailer)
        {
            this.pdfSettingsContainer = pdfSettingsContainer;
            this.imageSettingsContainer = imageSettingsContainer;
            this.emailSettingsContainer = emailSettingsContainer;
            this.dialogHelper = dialogHelper;
            this.fileNamePlaceholders = fileNamePlaceholders;
            this.changeTracker = changeTracker;
            this.operationFactory = operationFactory;
            this.formFactory = formFactory;
            this.ocrDependencyManager = ocrDependencyManager;
            this.emailer = emailer;
        }

        public bool SavePDF(List<ScannedImage> images, ISaveNotify notify)
        {
            if (images.Any())
            {
                string savePath;

                var pdfSettings = pdfSettingsContainer.PdfSettings;
                if (pdfSettings.SkipSavePrompt && Path.IsPathRooted(pdfSettings.DefaultFileName))
                {
                    savePath = pdfSettings.DefaultFileName;
                }
                else
                {
                    if (!dialogHelper.PromptToSavePdf(pdfSettings.DefaultFileName, out savePath))
                    {
                        return false;
                    }
                }

                var subSavePath = fileNamePlaceholders.SubstitutePlaceholders(savePath, DateTime.Now);
                if (ExportPDF(subSavePath, images, false))
                {
                    changeTracker.HasUnsavedChanges = false;
                    notify?.PdfSaved(subSavePath);
                    return true;
                }
            }
            return false;
        }

        public bool ExportPDF(string filename, List<ScannedImage> images, bool email)
        {
            var op = operationFactory.Create<SavePdfOperation>();
            var progressForm = formFactory.Create<FProgress>();
            progressForm.Operation = op;

            var pdfSettings = pdfSettingsContainer.PdfSettings;
            pdfSettings.Metadata.Creator = MiscResources.NAPS2;
            if (op.Start(filename, DateTime.Now, images, pdfSettings, ocrDependencyManager.DefaultLanguageCode, email))
            {
                progressForm.ShowDialog();
            }
            return op.Status.Success;
        }

        public bool SaveImages(List<ScannedImage> images, ISaveNotify notify)
        {
            if (images.Any())
            {
                string savePath;

                var imageSettings = imageSettingsContainer.ImageSettings;
                if (imageSettings.SkipSavePrompt && Path.IsPathRooted(imageSettings.DefaultFileName))
                {
                    savePath = imageSettings.DefaultFileName;
                }
                else
                {
                    if (!dialogHelper.PromptToSaveImage(imageSettings.DefaultFileName, out savePath))
                    {
                        return false;
                    }
                }

                var op = operationFactory.Create<SaveImagesOperation>();
                var progressForm = formFactory.Create<FProgress>();
                progressForm.Operation = op;
                progressForm.Start = () => op.Start(savePath, DateTime.Now, images);
                progressForm.ShowDialog();
                if (op.Status.Success)
                {
                    changeTracker.HasUnsavedChanges = false;
                    notify?.ImagesSaved(images.Count, op.FirstFileSaved);
                    return true;
                }
            }
            return false;
        }

        public bool EmailPDF(List<ScannedImage> images)
        {
            if (images.Any())
            {
                var emailSettings = emailSettingsContainer.EmailSettings;
                var invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars());
                var attachmentName = new string(emailSettings.AttachmentName.Where(x => !invalidChars.Contains(x)).ToArray());
                if (string.IsNullOrEmpty(attachmentName))
                {
                    attachmentName = "Scan.pdf";
                }
                if (!attachmentName.EndsWith(".pdf", StringComparison.InvariantCultureIgnoreCase))
                {
                    attachmentName += ".pdf";
                }
                attachmentName = fileNamePlaceholders.SubstitutePlaceholders(attachmentName, DateTime.Now, false);

                var tempFolder = new DirectoryInfo(Path.Combine(Paths.Temp, Path.GetRandomFileName()));
                tempFolder.Create();
                try
                {
                    string targetPath = Path.Combine(tempFolder.FullName, attachmentName);
                    if (!ExportPDF(targetPath, images, true))
                    {
                        // Cancel or error
                        return false;
                    }
                    var message = new EmailMessage
                    {
                        Attachments =
                        {
                            new EmailAttachment
                            {
                                FilePath = targetPath,
                                AttachmentName = attachmentName
                            }
                        }
                    };

                    if (emailer.SendEmail(message))
                    {
                        changeTracker.HasUnsavedChanges = false;
                        return true;
                    }
                }
                finally
                {
                    tempFolder.Delete(true);
                }
            }
            return false;
        }
    }
}
