using Autofac;
using NAPS2.Images.Wpf;
using NAPS2.ImportExport.Email.Mapi;

namespace NAPS2.Modules;

public class WpfImagesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<WpfImageContext>().As<ImageContext>();
        builder.RegisterType<WpfImageContext>().AsSelf();
        builder.RegisterType<MapiWrapper>().As<IMapiWrapper>();
    }
}
