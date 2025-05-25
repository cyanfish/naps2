using Autofac;
using NAPS2.EtoForms.Ui;
using NAPS2.ImportExport;

namespace NAPS2.Modules;

public class GtkModule : GuiModule
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterType<LinuxApplicationLifecycle>().As<ApplicationLifecycle>();
        builder.RegisterType<GtkScannedImagePrinter>().As<IScannedImagePrinter>();
        builder.RegisterType<LinuxServiceManager>().As<IOsServiceManager>();
        builder.RegisterType<LinuxOpenWith>().As<IOsOpenWith>();

        builder.RegisterType<GtkDesktopForm>().As<DesktopForm>();
        builder.RegisterType<GtkPreviewForm>().As<PreviewForm>();
    }
}
