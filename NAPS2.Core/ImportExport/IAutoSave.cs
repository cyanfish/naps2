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
        void AutoSave(AutoSaveSettings settings, List<IScannedImage> images);
    }

    public class WinFormsAutoSave : IAutoSave
    {
        private readonly IOperationFactory operationFactory;
        private readonly IFormFactory formFactory;
        private readonly PdfSettingsContainer pdfSettingsContainer;
        private readonly IUserConfigManager userConfigManager;
        private readonly IErrorOutput errorOutput;
        private readonly AppConfigManager appConfigManager;

        public WinFormsAutoSave(IOperationFactory operationFactory, IFormFactory formFactory, PdfSettingsContainer pdfSettingsContainer, IUserConfigManager userConfigManager, IErrorOutput errorOutput, AppConfigManager appConfigManager)
        {
            this.operationFactory = operationFactory;
            this.formFactory = formFactory;
            this.pdfSettingsContainer = pdfSettingsContainer;
            this.userConfigManager = userConfigManager;
            this.errorOutput = errorOutput;
            this.appConfigManager = appConfigManager;
        }

        public void AutoSave(AutoSaveSettings settings, List<IScannedImage> images)
        {
            if (appConfigManager.Config.DisableAutoSave)
            {
                return;
            }
            try
            {
                var form = formFactory.Create<FProgress>();
                var extension = Path.GetExtension(settings.FilePath);
                if (extension != null && extension.Equals(".pdf", StringComparison.InvariantCultureIgnoreCase))
                {
                    var op = operationFactory.Create<SavePdfOperation>();
                    form.Operation = op;
                    var ocrLanguageCode = userConfigManager.Config.EnableOcr ? userConfigManager.Config.OcrLanguageCode : null;
                    if (op.Start(settings.FilePath, DateTime.Now, images, pdfSettingsContainer.PdfSettings, ocrLanguageCode, false))
                    {
                        form.ShowDialog();
                    }
                }
                else
                {
                    var op = operationFactory.Create<SaveImagesOperation>();
                    form.Operation = op;
                    if (op.Start(settings.FilePath, DateTime.Now, images))
                    {
                        form.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException(MiscResources.AutoSaveError, ex);
                errorOutput.DisplayError(MiscResources.AutoSaveError);
            }
        }
    }

    public class ConsoleAutoSave : IAutoSave
    {
        public void AutoSave(AutoSaveSettings settings, List<IScannedImage> images)
        {
            // Not supported in NAPS2.Console
        }
    }
}
