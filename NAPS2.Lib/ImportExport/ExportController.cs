using NAPS2.EtoForms;
using NAPS2.EtoForms.Notifications;
using NAPS2.EtoForms.Ui;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.Pdf;

namespace NAPS2.ImportExport;

public class ExportController : IExportController
{
    private readonly DialogHelper _dialogHelper;
    private readonly IOperationFactory _operationFactory;
    private readonly IFormFactory _formFactory;
    private readonly OperationProgress _operationProgress;
    private readonly Naps2Config _config;
    private readonly UiImageList _imageList;
    private readonly NameManager _nameManager;

    public ExportController(DialogHelper dialogHelper, IOperationFactory operationFactory, IFormFactory formFactory,
        OperationProgress operationProgress, Naps2Config config, UiImageList imageList, NameManager nameManager)
    {
        _dialogHelper = dialogHelper;
        _operationFactory = operationFactory;
        _formFactory = formFactory;
        _operationProgress = operationProgress;
        _config = config;
        _imageList = imageList;
        _nameManager = nameManager;
    }

    public async Task<bool> SavePdf(ICollection<UiImage> uiImages, ISaveNotify notify)
    {
        using var images = GetSnapshots(uiImages);
        if (!images.Any())
        {
            return false;
        }

        string savePath;
        var defaultFileName = _config.Get(c => c.PdfSettings.DefaultFileName);
        if (_config.Get(c => c.PdfSettings.SkipSavePrompt) && Path.IsPathRooted(defaultFileName))
        {
            savePath = defaultFileName!;
        }
        else
        {
            if (!_dialogHelper.PromptToSavePdf(GetDefaultPath(defaultFileName, uiImages, true), out savePath!))
            {
                return false;
            }
        }

        if (await DoSavePdf(images, notify, savePath))
        {
            MaybeDeleteAfterSaving(uiImages);
            return true;
        }
        return false;
    }

    public async Task<bool> SaveImages(ICollection<UiImage> uiImages, ISaveNotify notify)
    {
        using var images = GetSnapshots(uiImages);
        if (!images.Any())
        {
            return false;
        }

        string savePath;
        var defaultFileName = _config.Get(c => c.ImageSettings.DefaultFileName);
        if (_config.Get(c => c.ImageSettings.SkipSavePrompt) &&
            Path.IsPathRooted(defaultFileName))
        {
            savePath = defaultFileName!;
        }
        else
        {
            if (!_dialogHelper.PromptToSaveImage(GetDefaultPath(defaultFileName, uiImages, false), out savePath!))
            {
                return false;
            }
        }

        if (await DoSaveImages(images, notify, savePath))
        {
            MaybeDeleteAfterSaving(uiImages);
            return true;
        }
        return false;
    }

    public async Task<bool> SavePdfOrImages(ICollection<UiImage> uiImages, ISaveNotify notify)
    {
        // Note this path bypasses some of the pdf/image save options (e.g. default file name)
        using var images = GetSnapshots(uiImages);

        string savePath;
        var pdfDefaultFileName = _config.Get(c => c.PdfSettings.DefaultFileName);
        var imageDefaultFileName = _config.Get(c => c.ImageSettings.DefaultFileName);
        if (_config.Get(c => c.PdfSettings.SkipSavePrompt) && Path.IsPathRooted(pdfDefaultFileName))
        {
            savePath = pdfDefaultFileName!;
        }
        else if (_config.Get(c => c.ImageSettings.SkipSavePrompt) && Path.IsPathRooted(imageDefaultFileName))
        {
            savePath = imageDefaultFileName!;
        }
        else
        {
            var defaultFileName = string.IsNullOrWhiteSpace(pdfDefaultFileName)
                ? imageDefaultFileName
                : pdfDefaultFileName;
            if (!_dialogHelper.PromptToSavePdfOrImage(GetDefaultPath(defaultFileName, uiImages, null), out savePath!))
            {
                return false;
            }
        }

        if (Path.GetExtension(savePath).ToLowerInvariant() == ".pdf"
                ? await DoSavePdf(images, notify, savePath)
                : await DoSaveImages(images, notify, savePath))
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

        if (!_config.User.Has(c => c.EmailSetup.ProviderType))
        {
            // First email attempt; prompt for a provider
            var form = _formFactory.Create<EmailProviderForm>();
            Invoker.Current.Invoke(() => form.ShowModal());
            if (!form.Result)
            {
                return false;
            }
        }

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
        attachmentName = _nameManager.Get(attachmentName,
            new NameContext { DateTime = DateTime.Now, IsFilePath = false });

        if (await DoEmailPdf(images, attachmentName))
        {
            MaybeDeleteAfterSaving(uiImages);
            return true;
        }
        return false;
    }

    private async Task<bool> DoSavePdf(IList<ProcessedImage> images, ISaveNotify notify, string savePath)
    {
        var state = _imageList.CurrentState;
        // The save dialog already confirms overwriting
        var (success, firstFileSaved) = await RunSavePdfOperation(savePath, images,
            saveParams: new SaveParams { OverwriteMode = OverwriteMode.Confirmed });
        if (success)
        {
            _imageList.MarkSaved(state, images);
            notify.PdfSaved(firstFileSaved!);
            return true;
        }
        return false;
    }

    private async Task<bool> DoSaveImages(IList<ProcessedImage> images, ISaveNotify notify, string savePath)
    {
        var op = _operationFactory.Create<SaveImagesOperation>();
        var state = _imageList.CurrentState;
        // The save dialog already confirms overwriting
        if (op.Start(savePath, images, _config.Get(c => c.ImageSettings),
                new SaveParams { OverwriteMode = OverwriteMode.Confirmed }))
        {
            _operationProgress.ShowProgress(op);
        }
        if (await op.Success)
        {
            _imageList.MarkSaved(state, images);
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

            if ((await RunSavePdfOperation(targetPath, images, new EmailMessage())).Success)
            {
                _imageList.MarkSaved(state, images);
                return true;
            }
        }
        finally
        {
            tempFolder.Delete(true);
        }
        return false;
    }

    private async Task<(bool Success, string? FirstFileSaved)> RunSavePdfOperation(string filename,
        IList<ProcessedImage> images, EmailMessage? emailMessage = null, SaveParams? saveParams = null)
    {
        var op = _operationFactory.Create<SavePdfOperation>();

        if (op.Start(filename, images, _config.Get(c => c.PdfSettings), _config.DefaultOcrParams(), emailMessage,
                saveParams))
        {
            _operationProgress.ShowProgress(op);
        }
        return (await op.Success, op.FirstFileSaved);
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

    private string? GetDefaultPath(string? defaultFileName, ICollection<UiImage> uiImages, bool? pdf)
    {
        if (!string.IsNullOrEmpty(defaultFileName))
        {
            return defaultFileName;
        }
        var originalFilePaths = uiImages
            .Select(x => x.GetImageWeakReference().ProcessedImage.PostProcessingData.OriginalFilePath)
            .WhereNotNull()
            .Where(x => pdf == null || pdf == (Path.GetExtension(x).ToLowerInvariant() == ".pdf"))
            .Distinct()
            .ToList();
        if (originalFilePaths.Count == 1)
        {
            return originalFilePaths[0];
        }
        return null;
    }
}