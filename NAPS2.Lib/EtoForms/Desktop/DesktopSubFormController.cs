using NAPS2.EtoForms.Ui;
using NAPS2.Ocr;

namespace NAPS2.EtoForms.Desktop;

public class DesktopSubFormController : IDesktopSubFormController
{
    private readonly IFormFactory _formFactory;
    private readonly UiImageList _imageList;
    private readonly DesktopImagesController _desktopImagesController;
    private readonly TesseractLanguageManager _tesseractLanguageManager;

    public DesktopSubFormController(IFormFactory formFactory, UiImageList imageList,
        DesktopImagesController desktopImagesController, TesseractLanguageManager tesseractLanguageManager)
    {
        _formFactory = formFactory;
        _imageList = imageList;
        _desktopImagesController = desktopImagesController;
        _tesseractLanguageManager = tesseractLanguageManager;
    }

    private Func<ListSelection<UiImage>>? SelectionFunc { get; init; }

    private ListSelection<UiImage> Selection => SelectionFunc?.Invoke() ?? _imageList.Selection;

    public IDesktopSubFormController WithSelection(Func<ListSelection<UiImage>> selectionFunc)
    {
        return new DesktopSubFormController(_formFactory, _imageList, _desktopImagesController,
            _tesseractLanguageManager)
        {
            SelectionFunc = selectionFunc
        };
    }

    public void ShowCropForm() => ShowImageForm<CropForm>();
    public void ShowBrightnessContrastForm() => ShowImageForm<BrightContForm>();
    public void ShowHueSaturationForm() => ShowImageForm<HueSatForm>();
    public void ShowBlackWhiteForm() => ShowImageForm<BlackWhiteForm>();
    public void ShowSharpenForm() => ShowImageForm<SharpenForm>();
    public void ShowSplitForm() => ShowImageForm<SplitForm>();
    public void ShowRotateForm() => ShowImageForm<RotateForm>();

    public void ShowCombineForm()
    {
        if (_imageList.Images.Count < 2) return;
        ShowImageForm<CombineForm>();
    }

    private void ShowImageForm<T>() where T : ImageFormBase
    {
        var selection = Selection;
        if (selection.Any())
        {
            var form = _formFactory.Create<T>();
            form.Image = selection.First();
            form.SelectedImages = selection.ToList();
            form.ShowModal();
        }
    }

    public void ShowProfilesForm()
    {
        var form = _formFactory.Create<ProfilesForm>();
        form.ImageCallback = _desktopImagesController.ReceiveScannedImage();
        form.ShowModal();
    }

    public void ShowOcrForm()
    {
        if (_tesseractLanguageManager.InstalledLanguages.Any())
        {
            _formFactory.Create<OcrSetupForm>().ShowModal();
        }
        else
        {
            _formFactory.Create<OcrDownloadForm>().ShowModal();
            if (_tesseractLanguageManager.InstalledLanguages.Any())
            {
                _formFactory.Create<OcrSetupForm>().ShowModal();
            }
        }
    }

    public void ShowBatchScanForm()
    {
        var form = _formFactory.Create<BatchScanForm>();
        form.ImageCallback = _desktopImagesController.ReceiveScannedImage();
        form.ShowModal();
    }

    public void ShowScannerSharingForm()
    {
        var form = _formFactory.Create<ScannerSharingForm>();
        form.ShowModal();
    }

    public void ShowViewerForm()
    {
        var selected = Selection.FirstOrDefault();
        if (selected != null)
        {
            using var viewer = _formFactory.Create<PreviewForm>();
            viewer.CurrentImage = selected;
            viewer.ShowModal();
        }
    }

    public void ShowPdfSettingsForm()
    {
        _formFactory.Create<PdfSettingsForm>().ShowModal();
    }

    public void ShowImageSettingsForm()
    {
        _formFactory.Create<ImageSettingsForm>().ShowModal();
    }

    public void ShowEmailSettingsForm()
    {
        _formFactory.Create<EmailSettingsForm>().ShowModal();
    }

    public void ShowSettingsForm()
    {
        _formFactory.Create<SettingsForm>().ShowModal();
    }

    public void ShowAboutForm()
    {
        _formFactory.Create<AboutForm>().ShowModal();
    }
}