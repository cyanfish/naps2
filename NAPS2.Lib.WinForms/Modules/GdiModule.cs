using Autofac;
using NAPS2.Images.Gdi;
using NAPS2.ImportExport.Email.Mapi;

namespace NAPS2.Modules;

public class GdiModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<GdiImageContext>().As<ImageContext>();
        builder.RegisterType<GdiImageContext>().AsSelf();
        builder.RegisterType<MapiWrapper>().As<IMapiWrapper>();
    }
}