using Autofac;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Gtk;
using NAPS2.EtoForms.Ui;
using NAPS2.Images.Gtk;
using NAPS2.ImportExport;
using NAPS2.Update;

namespace NAPS2.Modules;

public class GtkModule : GuiModule
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterType<StubNotificationManager>().As<INotificationManager>().SingleInstance();
        builder.RegisterType<StubScannedImagePrinter>().As<IScannedImagePrinter>();
        builder.RegisterType<GtkDarkModeProvider>().As<IDarkModeProvider>();
        builder.RegisterType<GtkImageContext>().As<ImageContext>();
        builder.RegisterType<GtkImageContext>().AsSelf();

        builder.RegisterType<GtkDesktopForm>().As<DesktopForm>();
        builder.RegisterType<GtkPreviewForm>().As<PreviewForm>();

        EtoPlatform.Current = new GtkEtoPlatform();
    }
}

public class StubScannedImagePrinter : IScannedImagePrinter
{
    public Task<bool> PromptToPrint(IList<ProcessedImage> images, IList<ProcessedImage> selectedImages)
    {
        return Task.FromResult(false);
    }
}

public class StubNotificationManager : INotificationManager
{
    public void PdfSaved(string path)
    {
    }

    public void ImagesSaved(int imageCount, string path)
    {
    }

    public void DonatePrompt()
    {
    }

    public void OperationProgress(OperationProgress opModalProgress, IOperation op)
    {
    }

    public void UpdateAvailable(IUpdateChecker updateChecker, UpdateInfo update)
    {
    }

    public void Rebuild()
    {
    }
}
