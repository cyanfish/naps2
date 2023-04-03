using Autofac;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Ui;
using NAPS2.EtoForms.WinForms;
using NAPS2.ImportExport;
using NAPS2.WinForms;

namespace NAPS2.Modules;

public class WinFormsModule : GuiModule
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterType<PrintDocumentPrinter>().As<IScannedImagePrinter>();
        builder.RegisterType<WinFormsDarkModeProvider>().As<IDarkModeProvider>().SingleInstance();

        builder.RegisterType<WinFormsDesktopForm>().As<DesktopForm>();

        EtoPlatform.Current = new WinFormsEtoPlatform();
        // TODO: Can we add a test for this?
        builder.RegisterBuildCallback(ctx =>
            Log.EventLogger = ctx.Resolve<WindowsEventLogger>());
    }
}