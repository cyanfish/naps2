using NAPS2.EtoForms.Ui;

namespace NAPS2.WinForms;

public class DesktopSubFormController
{
    private readonly IFormFactory _formFactory;
    private readonly UiImageList _imageList;
    private readonly DesktopImagesController _desktopImagesController;

    public DesktopSubFormController(IFormFactory formFactory, UiImageList imageList,
        DesktopImagesController desktopImagesController)
    {
        _formFactory = formFactory;
        _imageList = imageList;
        _desktopImagesController = desktopImagesController;
    }

    public void ShowImageForm<T>() where T : ImageForm
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

    public void ShowAboutForm()
    {
        _formFactory.Create<AboutForm>().ShowModal();
    }

    public void ShowSettingsForm()
    {
        // FormFactory.Create<FSettings>().ShowDialog();
    }
}