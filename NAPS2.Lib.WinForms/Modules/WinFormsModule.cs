using Autofac;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Ui;
using NAPS2.EtoForms.WinForms;
using NAPS2.ImportExport;
using NAPS2.Platform.Windows;

namespace NAPS2.Modules;

public class WinFormsModule : GuiModule
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterType<WindowsApplicationLifecycle>().As<ApplicationLifecycle>();
        builder.RegisterType<PrintDocumentPrinter>().As<IScannedImagePrinter>();
        // TODO: Change this when implementing dark mode on Windows
        builder.RegisterType<StubDarkModeProvider>().As<IDarkModeProvider>().SingleInstance();
        builder.RegisterType<WindowsServiceManager>().As<IOsServiceManager>().SingleInstance();

        builder.RegisterType<WinFormsDesktopForm>().As<DesktopForm>();
        builder.RegisterType<WinFormsPreviewForm>().As<PreviewForm>();

        // TODO: Can we add a test for this?
        builder.RegisterBuildCallback(ctx =>
            Log.EventLogger = ctx.Resolve<WindowsEventLogger>());
    }
}