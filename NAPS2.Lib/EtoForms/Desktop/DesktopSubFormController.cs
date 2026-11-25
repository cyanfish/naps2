using NAPS2.EtoForms.Ui;
using NAPS2.Ocr;

namespace NAPS2.EtoForms.Desktop;

public class DesktopSubFormController : IDesktopSubFormController
{
    private readonly Naps2Config _config;
    private readonly IFormFactory _formFactory;
    private readonly IImageEditorFactory _imageEditorFactory;
    private readonly UiImageList _imageList;
    private readonly DesktopImagesController _desktopImagesController;
    private readonly TesseractLanguageManager _tesseractLanguageManager;
    private readonly DesktopFormProvider _desktopFormProvider;

    public DesktopSubFormController(Naps2Config config, IFormFactory formFactory,
        IImageEditorFactory imageEditorFactory, UiImageList imageList,
        DesktopImagesController desktopImagesController, TesseractLanguageManager tesseractLanguageManager,
        DesktopFormProvider desktopFormProvider)
    {
        _config = config;
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
        return new DesktopSubFormController(_config, _formFactory, _imageEditorFactory, _imageList,
            _desktopImagesController, _tesseractLanguageManager, _desktopFormProvider)
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

    public void ShowPreview()
    {
        if (_config.Get(c => c.PreviewLastShownInPane))
        {
            ShowPreviewPane();
        }
        else
        {
            ShowPreviewWindow();
        }
    }

    public void ShowPreviewPane()
    {
        if (Selection.FirstOrDefault() is not { } image) return;
        _desktopFormProvider.DesktopForm.PreviewPane.Show(image);
    }

    public void ShowPreviewWindow()
    {
        if (Selection.FirstOrDefault() is not { } image) return;
        var pane = _desktopFormProvider.DesktopForm.PreviewPane;
        var editorType = pane.CurrentImage == image ? pane.ActiveEditorType : null;
        pane.Hide();
        using var viewer = _formFactory.Create<PreviewForm>();
        viewer.CurrentImage = image;
        viewer.EditorType = editorType;
        viewer.ShowModal();
        if (viewer.ContinueInPane)
        {
            pane.Show(viewer.CurrentImage, viewer.EditorType);
        }
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