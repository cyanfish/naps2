using Autofac;
using NAPS2.EtoForms.Ui;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;

namespace NAPS2.Modules;

public class GtkModule : GuiModule
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterType<LinuxApplicationLifecycle>().As<ApplicationLifecycle>();
        builder.RegisterType<GtkScannedImagePrinter>().As<IScannedImagePrinter>();
        builder.RegisterType<LinuxServiceManager>().As<IOsServiceManager>();
        builder.RegisterType<LinuxOpenWith>().As<IOpenWith>();
        builder.RegisterType<ThunderbirdEmailProvider>().As<IEmailProvider>().WithParameter("systemDefault", true);
        builder.RegisterType<StubSystemEmailClients>().As<ISystemEmailClients>();

        builder.RegisterType<GtkDesktopForm>().As<DesktopForm>();
        builder.RegisterType<GtkPreviewForm>().As<PreviewForm>();
    }
}
