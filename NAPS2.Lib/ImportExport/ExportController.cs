using NAPS2.EtoForms;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;

namespace NAPS2.ImportExport;

public class ExportController : IExportController
{
    private readonly DialogHelper _dialogHelper;
    private readonly IOperationFactory _operationFactory;
    private readonly IFormFactory _formFactory;
    private readonly OperationProgress _operationProgress;
    private readonly Naps2Config _config;
    private readonly UiImageList _imageList;

    public ExportController(DialogHelper dialogHelper, IOperationFactory operationFactory, IFormFactory formFactory,
        OperationProgress operationProgress, Naps2Config config, UiImageList imageList)
    {
        _dialogHelper = dialogHelper;
        _operationFactory = operationFactory;
        _formFactory = formFactory;
        _operationProgress = operationProgress;
        _config = config;
        _imageList = imageList;
    }

    public async Task<bool> SavePdf(ICollection<UiImage> uiImages, ISaveNotify notify)
    {
        using var images = GetSnapshots(uiImages);
        if (images.Any())
        {
            string savePath;

            var defaultFileName = _config.Get(c => c.PdfSettings.DefaultFileName);
            if (_config.Get(c => c.PdfSettings.SkipSavePrompt) && Path.IsPathRooted(defaultFileName))
            {
                savePath = defaultFileName!;
            }
            else
            {
                if (!_dialogHelper.PromptToSavePdf(defaultFileName, out savePath!))
                {
                    return false;
                }
            }

            if (await DoSavePdf(images, notify, savePath))
            {
                MaybeDeleteAfterSaving(uiImages);
                return true;
            }
        }
        return false;
    }

    public async Task<bool> SaveImages(ICollection<UiImage> uiImages, ISaveNotify notify)
    {
        using var images = GetSnapshots(uiImages);
        if (images.Any())
        {
            string savePath;

            if (_config.Get(c => c.ImageSettings.SkipSavePrompt) &&
                Path.IsPathRooted(_config.Get(c => c.ImageSettings.DefaultFileName)))
            {
                savePath = _config.Get(c => c.ImageSettings.DefaultFileName)!;
            }
            else
            {
                if (!_dialogHelper.PromptToSaveImage(_config.Get(c => c.ImageSettings.DefaultFileName), out savePath!))
                {
                    return false;
                }
            }

            if (await DoSaveImages(images, notify, savePath))
            {
                MaybeDeleteAfterSaving(uiImages);
                return true;
            }
        }
        return false;
    }

    public async Task<bool> SavePdfOrImages(ICollection<UiImage> uiImages, ISaveNotify notify)
    {
        // Note this path bypasses some of the pdf/image save options (e.g. default file name)
        using var images = GetSnapshots(uiImages);
        if (!_dialogHelper.PromptToSavePdfOrImage(null, out var savePath))
        {
            return false;
        }
        if (Path.GetExtension(savePath!).ToLowerInvariant() == ".pdf"
                ? await DoSavePdf(images, notify, savePath!)
                : await DoSaveImages(images, notify, savePath!))
        {
            MaybeDeleteAfterSaving(uiImages);
            return true;
        }
        return false;
    }

    public async Task<bool> EmailPdf(ICollection<UiImage> uiImages)
    {
        using var images = GetSnapshots(uiImages);
        if (!images.Any())
        {
            return false;
        }

        // TODO: What?
        // if (_config == null)
        // {
        //     // First run; prompt for a
        //     var form = _formFactory.Create<FEmailProvider>();
        //     if (form.ShowDialog() != DialogResult.OK)
        //     {
        //         return false;
        //     }
        // }

        var invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars());
        var attachmentName = new string(_config.Get(c => c.EmailSettings.AttachmentName)
            .Where(x => !invalidChars.Contains(x)).ToArray());
        if (string.IsNullOrEmpty(attachmentName))
        {
            attachmentName = "Scan.pdf";
        }
        if (!attachmentName.EndsWith(".pdf", StringComparison.InvariantCultureIgnoreCase))
        {
            attachmentName += ".pdf";
        }
        attachmentName = Placeholders.All.Substitute(attachmentName, false);

        if (await DoEmailPdf(images, attachmentName))
        {
            MaybeDeleteAfterSaving(uiImages);
            return true;
        }
        return false;
    }

    private async Task<bool> DoSavePdf(IList<ProcessedImage> images, ISaveNotify notify, string savePath)
    {
        var subSavePath = Placeholders.All.Substitute(savePath);
        var state = _imageList.CurrentState;
        if (await RunSavePdfOperation(subSavePath, images, false, null))
        {
            _imageList.SavedState = state;
            notify?.PdfSaved(subSavePath);
            return true;
        }
        return false;
    }

    private async Task<bool> DoSaveImages(IList<ProcessedImage> images, ISaveNotify notify, string savePath)
    {
        var op = _operationFactory.Create<SaveImagesOperation>();
        var state = _imageList.CurrentState;
        if (op.Start(savePath, Placeholders.All.WithDate(DateTime.Now), images, _config.Get(c => c.ImageSettings), savePath))
        {
            _operationProgress.ShowProgress(op);
        }
        if (await op.Success)
        {
            _imageList.SavedState = state;
            notify.ImagesSaved(images.Count, op.FirstFileSaved!);
            return true;
        }
        return false;
    }

    private async Task<bool> DoEmailPdf(IList<ProcessedImage> images, string attachmentName)
    {
        var tempFolder = new DirectoryInfo(Path.Combine(Paths.Temp, Path.GetRandomFileName()));
        tempFolder.Create();
        try
        {
            string targetPath = Path.Combine(tempFolder.FullName, attachmentName);
            var state = _imageList.CurrentState;

            var message = new EmailMessage
            {
                Attachments = { new EmailAttachment(targetPath, attachmentName) }
            };

            if (await RunSavePdfOperation(targetPath, images, true, message))
            {
                _imageList.SavedState = state;
                return true;
            }
        }
        finally
        {
            tempFolder.Delete(true);
        }
        return false;
    }

    private async Task<bool> RunSavePdfOperation(string filename, IList<ProcessedImage> images, bool email,
        EmailMessage? emailMessage)
    {
        var op = _operationFactory.Create<SavePdfOperation>();

        if (op.Start(filename, Placeholders.All.WithDate(DateTime.Now), images, _config.Get(c => c.PdfSettings),
                _config.DefaultOcrParams(), email, emailMessage, filename))
        {
            _operationProgress.ShowProgress(op);
        }
        return await op.Success;
    }

    private DisposableList<ProcessedImage> GetSnapshots(IEnumerable<UiImage> uiImages)
    {
        return uiImages.Select(x => x.GetClonedImage()).ToDisposableList();
    }

    private void MaybeDeleteAfterSaving(ICollection<UiImage> uiImages)
    {
        if (_config.Get(c => c.DeleteAfterSaving))
        {
            _imageList.Mutate(new ImageListMutation.DeleteSelected(), ListSelection.From(uiImages));
        }
    }
}