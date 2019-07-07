using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Ocr;
using NAPS2.Operation;
using NAPS2.Images;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public class WinFormsExportHelper
    {
        private readonly ConfigProvider<PdfSettings> pdfSettingsProvider;
        private readonly ConfigProvider<ImageSettings> imageSettingsProvider;
        private readonly ConfigProvider<EmailSettings> emailSettingsProvider;
        private readonly DialogHelper dialogHelper;
        private readonly ChangeTracker changeTracker;
        private readonly IOperationFactory operationFactory;
        private readonly IFormFactory formFactory;
        private readonly OperationProgress operationProgress;
        private readonly ConfigScopes configScopes;

        public WinFormsExportHelper(ConfigProvider<PdfSettings> pdfSettingsProvider, ConfigProvider<ImageSettings> imageSettingsProvider, ConfigProvider<EmailSettings> emailSettingsProvider, DialogHelper dialogHelper, ChangeTracker changeTracker, IOperationFactory operationFactory, IFormFactory formFactory, OperationProgress operationProgress, ConfigScopes configScopes)
        {
            this.pdfSettingsProvider = pdfSettingsProvider;
            this.imageSettingsProvider = imageSettingsProvider;
            this.emailSettingsProvider = emailSettingsProvider;
            this.dialogHelper = dialogHelper;
            this.changeTracker = changeTracker;
            this.operationFactory = operationFactory;
            this.formFactory = formFactory;
            this.operationProgress = operationProgress;
            this.configScopes = configScopes;
        }

        public async Task<bool> SavePDF(List<ScannedImage> images, ISaveNotify notify)
        {
            if (images.Any())
            {
                string savePath;

                var defaultFileName = pdfSettingsProvider.Get(c => c.DefaultFileName);
                if (pdfSettingsProvider.Get(c => c.SkipSavePrompt) && Path.IsPathRooted(defaultFileName))
                {
                    savePath = defaultFileName;
                }
                else
                {
                    if (!dialogHelper.PromptToSavePdf(defaultFileName, out savePath))
                    {
                        return false;
                    }
                }

                var subSavePath = Placeholders.All.Substitute(savePath);
                var changeToken = changeTracker.State;
                if (await ExportPDF(subSavePath, images, false, null))
                {
                    changeTracker.Saved(changeToken);
                    notify?.PdfSaved(subSavePath);
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> ExportPDF(string filename, List<ScannedImage> images, bool email, EmailMessage emailMessage)
        {
            var op = operationFactory.Create<SavePdfOperation>();

            if (op.Start(filename, Placeholders.All.WithDate(DateTime.Now), images, pdfSettingsProvider, new OcrContext(configScopes.Provider.DefaultOcrParams()), email, emailMessage))
            {
                operationProgress.ShowProgress(op);
            }
            return await op.Success;
        }

        public async Task<bool> SaveImages(List<ScannedImage> images, ISaveNotify notify)
        {
            if (images.Any())
            {
                string savePath;

                if (imageSettingsProvider.Get(c => c.SkipSavePrompt) && Path.IsPathRooted(imageSettingsProvider.Get(c => c.DefaultFileName)))
                {
                    savePath = imageSettingsProvider.Get(c => c.DefaultFileName);
                }
                else
                {
                    if (!dialogHelper.PromptToSaveImage(imageSettingsProvider.Get(c => c.DefaultFileName), out savePath))
                    {
                        return false;
                    }
                }

                var op = operationFactory.Create<SaveImagesOperation>();
                var changeToken = changeTracker.State;
                if (op.Start(savePath, Placeholders.All.WithDate(DateTime.Now), images, imageSettingsProvider))
                {
                    operationProgress.ShowProgress(op);
                }
                if (await op.Success)
                {
                    changeTracker.Saved(changeToken);
                    notify?.ImagesSaved(images.Count, op.FirstFileSaved);
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> EmailPDF(List<ScannedImage> images)
        {
            if (!images.Any())
            {
                return false;
            }

            if (configScopes == null)
            {
                // First run; prompt for a 
                var form = formFactory.Create<FEmailProvider>();
                if (form.ShowDialog() != DialogResult.OK)
                {
                    return false;
                }
            }

            var invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars());
            var attachmentName = new string(emailSettingsProvider.Get(c => c.AttachmentName).Where(x => !invalidChars.Contains(x)).ToArray());
            if (string.IsNullOrEmpty(attachmentName))
            {
                attachmentName = "Scan.pdf";
            }
            if (!attachmentName.EndsWith(".pdf", StringComparison.InvariantCultureIgnoreCase))
            {
                attachmentName += ".pdf";
            }
            attachmentName = Placeholders.All.Substitute(attachmentName, false);

            var tempFolder = new DirectoryInfo(Path.Combine(Paths.Temp, Path.GetRandomFileName()));
            tempFolder.Create();
            try
            {
                string targetPath = Path.Combine(tempFolder.FullName, attachmentName);
                var changeToken = changeTracker.State;

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

                if (await ExportPDF(targetPath, images, true, message))
                {
                    changeTracker.Saved(changeToken);
                    return true;
                }
            }
            finally
            {
                tempFolder.Delete(true);
            }
            return false;
        }
    }
}
