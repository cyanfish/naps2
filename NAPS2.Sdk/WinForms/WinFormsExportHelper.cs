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

namespace NAPS2.WinForms;

public class WinFormsExportHelper
{
    private readonly IConfigProvider<PdfSettings> _pdfSettingsProvider;
    private readonly IConfigProvider<ImageSettings> _imageSettingsProvider;
    private readonly IConfigProvider<EmailSettings> _emailSettingsProvider;
    private readonly DialogHelper _dialogHelper;
    private readonly IOperationFactory _operationFactory;
    private readonly IFormFactory _formFactory;
    private readonly OperationProgress _operationProgress;
    private readonly ScopedConfig _config;
    private readonly ScannedImageList _scannedImageList;

    public WinFormsExportHelper(IConfigProvider<PdfSettings> pdfSettingsProvider, IConfigProvider<ImageSettings> imageSettingsProvider, IConfigProvider<EmailSettings> emailSettingsProvider, DialogHelper dialogHelper, IOperationFactory operationFactory, IFormFactory formFactory, OperationProgress operationProgress, ScopedConfig config, ScannedImageList scannedImageList)
    {
        _pdfSettingsProvider = pdfSettingsProvider;
        _imageSettingsProvider = imageSettingsProvider;
        _emailSettingsProvider = emailSettingsProvider;
        _dialogHelper = dialogHelper;
        _operationFactory = operationFactory;
        _formFactory = formFactory;
        _operationProgress = operationProgress;
        _config = config;
        _scannedImageList = scannedImageList;
    }

    public async Task<bool> SavePDF(List<ScannedImage> images, ISaveNotify notify)
    {
        if (images.Any())
        {
            string savePath;

            var defaultFileName = _pdfSettingsProvider.Get(c => c.DefaultFileName);
            if (_pdfSettingsProvider.Get(c => c.SkipSavePrompt) && Path.IsPathRooted(defaultFileName))
            {
                savePath = defaultFileName;
            }
            else
            {
                if (!_dialogHelper.PromptToSavePdf(defaultFileName, out savePath))
                {
                    return false;
                }
            }

            var subSavePath = Placeholders.All.Substitute(savePath);
            var state = _scannedImageList.CurrentState;
            if (await ExportPDF(subSavePath, images, false, null))
            {
                _scannedImageList.SavedState = state;
                notify?.PdfSaved(subSavePath);
                return true;
            }
        }
        return false;
    }

    public async Task<bool> ExportPDF(string filename, List<ScannedImage> images, bool email, EmailMessage emailMessage)
    {
        var op = _operationFactory.Create<SavePdfOperation>();

        if (op.Start(filename, Placeholders.All.WithDate(DateTime.Now), images, _pdfSettingsProvider, new OcrContext(_config.DefaultOcrParams()), email, emailMessage))
        {
            _operationProgress.ShowProgress(op);
        }
        return await op.Success;
    }

    public async Task<bool> SaveImages(List<ScannedImage> images, ISaveNotify notify)
    {
        if (images.Any())
        {
            string savePath;

            if (_imageSettingsProvider.Get(c => c.SkipSavePrompt) && Path.IsPathRooted(_imageSettingsProvider.Get(c => c.DefaultFileName)))
            {
                savePath = _imageSettingsProvider.Get(c => c.DefaultFileName);
            }
            else
            {
                if (!_dialogHelper.PromptToSaveImage(_imageSettingsProvider.Get(c => c.DefaultFileName), out savePath))
                {
                    return false;
                }
            }

            var op = _operationFactory.Create<SaveImagesOperation>();
            var state = _scannedImageList.CurrentState;
            if (op.Start(savePath, Placeholders.All.WithDate(DateTime.Now), images, _imageSettingsProvider))
            {
                _operationProgress.ShowProgress(op);
            }
            if (await op.Success)
            {
                _scannedImageList.SavedState = state;
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

        if (_config == null)
        {
            // First run; prompt for a 
            var form = _formFactory.Create<FEmailProvider>();
            if (form.ShowDialog() != DialogResult.OK)
            {
                return false;
            }
        }

        var invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars());
        var attachmentName = new string(_emailSettingsProvider.Get(c => c.AttachmentName).Where(x => !invalidChars.Contains(x)).ToArray());
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
            var state = _scannedImageList.CurrentState;

            var message = new EmailMessage
            {
                Attachments = { new EmailAttachment(targetPath, attachmentName) }
            };

            if (await ExportPDF(targetPath, images, true, message))
            {
                _scannedImageList.SavedState = state;
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