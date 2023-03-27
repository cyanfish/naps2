using Autofac;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Mac;
using NAPS2.EtoForms.Ui;
using NAPS2.Images.Mac;
using NAPS2.ImportExport;
using NAPS2.Pdf;
using NAPS2.Update;

namespace NAPS2.Modules;

public class MacModule : GuiModule
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterType<StubNotificationManager>().As<INotificationManager>().SingleInstance();
        builder.RegisterType<MacScannedImagePrinter>().As<IScannedImagePrinter>();
        builder.RegisterType<MacDarkModeProvider>().As<IDarkModeProvider>();
        builder.RegisterType<MacImageContext>().As<ImageContext>();
        builder.RegisterType<MacImageContext>().AsSelf();
        builder.RegisterType<MacIconProvider>().As<IIconProvider>();

        builder.RegisterType<MacDesktopForm>().As<DesktopForm>();
        builder.RegisterType<MacPreviewForm>().As<PreviewForm>();

        EtoPlatform.Current = new MacEtoPlatform();
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

public class StubPdfPasswordProvider : IPdfPasswordProvider
{
    public bool ProvidePassword(string fileName, int attemptCount, out string password)
    {
        password = null!;
        return false;
    }
}