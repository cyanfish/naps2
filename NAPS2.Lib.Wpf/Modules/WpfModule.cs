using Autofac;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Ui;
using NAPS2.EtoForms.Wpf;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Platform.Windows;

namespace NAPS2.Modules;

public class WpfModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<WindowsApplicationLifecycle>().As<ApplicationLifecycle>();
        builder.RegisterType<WpfScannedImagePrinter>().As<IScannedImagePrinter>();
        builder.RegisterType<WindowsServiceManager>().As<IOsServiceManager>().SingleInstance();
        builder.RegisterType<WindowsOpenWith>().As<IOpenWith>();
        builder.RegisterType<MapiEmailProvider>().As<IEmailProvider>().WithParameter("systemDefault", true);
        builder.RegisterType<MapiEmailClients>().As<ISystemEmailClients>();
        builder.RegisterType<FluentIconProvider>().As<IIconProvider>();

        builder.RegisterType<WpfDesktopForm>().As<DesktopForm>();

        builder.RegisterBuildCallback(ctx =>
            Log.EventLogger = ctx.Resolve<WindowsEventLogger>());
    }
}
