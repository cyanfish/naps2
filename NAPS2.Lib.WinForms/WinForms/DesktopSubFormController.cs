using NAPS2.EtoForms.Ui;
using NAPS2.Ocr;

namespace NAPS2.WinForms;

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

    public void ShowCropForm() => ShowImageForm<FCrop>();
    public void ShowBrightnessContrastForm() => ShowImageForm<FBrightnessContrast>();
    public void ShowHueSaturationForm() => ShowImageForm<FHueSaturation>();
    public void ShowBlackWhiteForm() => ShowImageForm<FBlackWhite>();
    public void ShowSharpenForm() => ShowImageForm<FSharpen>();
    public void ShowRotateForm() => ShowImageForm<FRotate>();

    private void ShowImageForm<T>() where T : ImageForm
    {
        var selection = _imageList.Selection.ToList();
        if (selection.Any())
        {
            var form = _formFactory.Create<T>();
            form.Image = selection.First();
            form.SelectedImages = selection.ToList();
            form.ShowDialog();
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
            _formFactory.Create<FOcrSetup>().ShowDialog();
        }
        else
        {
            _formFactory.Create<FOcrLanguageDownload>().ShowDialog();
            if (_tesseractLanguageManager.InstalledLanguages.Any())
            {
                _formFactory.Create<FOcrSetup>().ShowDialog();
            }
        }
    }

    public void ShowBatchScanForm()
    {
        var form = _formFactory.Create<FBatchScan>();
        form.ImageCallback = _desktopImagesController.ReceiveScannedImage();
        form.ShowDialog();
    }

    public void ShowViewerForm()
    {
        var selected = _imageList.Selection.FirstOrDefault();
        if (selected != null)
        {
            using var viewer = _formFactory.Create<FViewer>();
            viewer.CurrentImage = selected;
            viewer.ShowDialog();
        }
    }

    public void ShowPdfSettingsForm()
    {
        _formFactory.Create<FPdfSettings>().ShowDialog();
    }

    public void ShowImageSettingsForm()
    {
        _formFactory.Create<FImageSettings>().ShowDialog();
    }

    public void ShowEmailSettingsForm()
    {
        _formFactory.Create<FEmailSettings>().ShowDialog();
    }

    public void ShowAboutForm()
    {
        _formFactory.Create<AboutForm>().ShowModal();
    }

    public void ShowSettingsForm()
    {
        // FormFactory.Create<FSettings>().ShowDialog();
    }
}