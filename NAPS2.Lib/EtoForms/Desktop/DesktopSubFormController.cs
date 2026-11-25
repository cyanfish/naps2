using NAPS2.EtoForms.Ui;
using NAPS2.Ocr;

namespace NAPS2.EtoForms.Desktop;

public class DesktopSubFormController : IDesktopSubFormController
{
    private readonly IFormFactory _formFactory;
    private readonly IImageEditorFactory _imageEditorFactory;
    private readonly UiImageList _imageList;
    private readonly DesktopImagesController _desktopImagesController;
    private readonly TesseractLanguageManager _tesseractLanguageManager;
    private readonly DesktopFormProvider _desktopFormProvider;

    public DesktopSubFormController(IFormFactory formFactory, IImageEditorFactory imageEditorFactory,
        UiImageList imageList, DesktopImagesController desktopImagesController,
        TesseractLanguageManager tesseractLanguageManager, DesktopFormProvider desktopFormProvider)
    {
        _formFactory = formFactory;
        _imageEditorFactory = imageEditorFactory;
        _imageList = imageList;
        _desktopImagesController = desktopImagesController;
        _tesseractLanguageManager = tesseractLanguageManager;
        _desktopFormProvider = desktopFormProvider;
    }

    private Func<ListSelection<UiImage>>? SelectionFunc { get; init; }

    private Action<ImageEditor>? EditorHost { get; init; }

    private ListSelection<UiImage> Selection => SelectionFunc?.Invoke() ?? _imageList.Selection;

    public IDesktopSubFormController WithSelection(Func<ListSelection<UiImage>> selectionFunc,
        Action<ImageEditor>? editorHost = null)
    {
        return new DesktopSubFormController(_formFactory, _imageEditorFactory, _imageList, _desktopImagesController,
            _tesseractLanguageManager, _desktopFormProvider)
        {
            SelectionFunc = selectionFunc,
            EditorHost = editorHost
        };
    }

    public void ShowCropForm() => ShowImageEditor<CropEditor>();
    public void ShowBrightnessContrastForm() => ShowImageEditor<BrightContEditor>();
    public void ShowHueSaturationForm() => ShowImageEditor<HueSatEditor>();
    public void ShowBlackWhiteForm() => ShowImageEditor<BlackWhiteEditor>();
    public void ShowSharpenForm() => ShowImageEditor<SharpenEditor>();
    public void ShowCompressForm() => ShowImageEditor<CompressEditor>();
    public void ShowSplitForm() => ShowImageEditor<SplitEditor>();
    public void ShowRotateForm() => ShowImageEditor<RotateEditor>();

    public void ShowCombineForm()
    {
        if (_imageList.Images.Count < 2) return;
        ShowImageEditor<CombineEditor>();
    }

    private void ShowImageEditor<T>() where T : ImageEditor
    {
        var selection = Selection;
        if (selection.Any())
        {
            var editor = _imageEditorFactory.Create<T>();
            editor.Image = selection.First();
            editor.SelectedImages = selection.ToList();
            if (EditorHost != null)
            {
                EditorHost(editor);
                return;
            }
            var form = _formFactory.Create<ImageEditorForm>();
            form.Editor = editor;
            form.ShowModal();
        }
    }

    public void ShowManualDuplexForm()
    {
        _formFactory.Create<ManualDuplexForm>().ShowModal();
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

    public void ShowPreview(bool newWindow)
    {
        var image = Selection.FirstOrDefault();
        if (image == null) return;
        var pane = _desktopFormProvider.DesktopForm.PreviewPane;
        if (!newWindow)
        {
            pane.Show(image);
            return;
        }
        pane.Hide();
        using var viewer = _formFactory.Create<PreviewForm>();
        viewer.CurrentImage = image;
        viewer.ShowModal();
    }

    public void ShowPdfSettingsForm() => ShowSettingsForm<PdfSettingsPanel>();

    public void ShowImageSettingsForm() => ShowSettingsForm<ImageSettingsPanel>();

    public void ShowEmailSettingsForm() => ShowSettingsForm<EmailSettingsPanel>();

    public void ShowSettingsForm() => ShowSettingsForm<GeneralSettingsPanel>();

    public void ShowToolbarSettingsForm() => ShowSettingsForm<ToolbarSettingsPanel>();

    private void ShowSettingsForm<T>() where T : SettingsPanel
    {
        var form = _formFactory.Create<CombinedSettingsForm>();
        form.SelectPanel<T>();
        form.ShowModal();
    }

    public void ShowAboutForm()
    {
        _formFactory.Create<AboutForm>().ShowModal();
    }
}