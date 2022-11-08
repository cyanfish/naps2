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

    public void ShowCropForm()
    {
    }

    public void ShowBrightnessContrastForm()
    {
    }

    public void ShowHueSaturationForm()
    {
    }

    public void ShowBlackWhiteForm()
    {
    }

    public void ShowSharpenForm()
    {
    }

    public void ShowRotateForm()
    {
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
    }

    public void ShowImageSettingsForm()
    {
    }

    public void ShowEmailSettingsForm()
    {
    }

    public void ShowAboutForm()
    {
        _formFactory.Create<AboutForm>().ShowModal();
    }

    public void ShowSettingsForm()
    {
    }
}