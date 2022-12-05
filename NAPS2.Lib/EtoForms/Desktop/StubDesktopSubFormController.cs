using NAPS2.EtoForms.Ui;

namespace NAPS2.EtoForms.Desktop;

public class StubDesktopSubFormController : IDesktopSubFormController
{
    private readonly IFormFactory _formFactory;
    private readonly DesktopImagesController _desktopImagesController;
    private readonly UiImageList _imageList;

    public StubDesktopSubFormController(IFormFactory formFactory, DesktopImagesController desktopImagesController, UiImageList imageList)
    {
        _formFactory = formFactory;
        _desktopImagesController = desktopImagesController;
        _imageList = imageList;
    }

    private void ShowImageForm<T>() where T : ImageFormBase
    {
        var selection = _imageList.Selection;
        if (selection.Any())
        {
            var form = _formFactory.Create<T>();
            form.Image = selection.First();
            form.SelectedImages = selection.ToList();
            form.ShowModal();
        }
    }

    public void ShowCropForm()
    {
        ShowImageForm<CropForm>();
    }

    public void ShowBrightnessContrastForm()
    {
        ShowImageForm<BrightContForm>();
    }

    public void ShowHueSaturationForm()
    {
        ShowImageForm<HueSatForm>();
    }

    public void ShowBlackWhiteForm()
    {
        ShowImageForm<BlackWhiteForm>();
    }

    public void ShowSharpenForm()
    {
        ShowImageForm<SharpenForm>();
    }

    public void ShowRotateForm()
    {
        ShowImageForm<RotateForm>();
    }

    public void ShowProfilesForm()
    {
        var form = _formFactory.Create<ProfilesForm>();
        form.ImageCallback = _desktopImagesController.ReceiveScannedImage();
        form.ShowModal();
    }

    public void ShowOcrForm()
    {
        _formFactory.Create<OcrSetupForm>().ShowModal();
    }

    public void ShowBatchScanForm()
    {
    }

    public void ShowViewerForm()
    {
        var selected = _imageList.Selection.FirstOrDefault();
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

    public void ShowAboutForm()
    {
        _formFactory.Create<AboutForm>().ShowModal();
    }

    public void ShowSettingsForm()
    {
    }
}