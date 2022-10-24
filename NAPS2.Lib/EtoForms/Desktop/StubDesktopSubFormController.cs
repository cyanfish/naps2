using NAPS2.EtoForms.Ui;

namespace NAPS2.EtoForms.Desktop;

public class StubDesktopSubFormController : IDesktopSubFormController
{
    private readonly IFormFactory _formFactory;
    private readonly DesktopImagesController _desktopImagesController;

    public StubDesktopSubFormController(IFormFactory formFactory, DesktopImagesController desktopImagesController)
    {
        _formFactory = formFactory;
        _desktopImagesController = desktopImagesController;
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