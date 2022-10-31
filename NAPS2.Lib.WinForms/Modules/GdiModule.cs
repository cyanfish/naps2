using Autofac;
using NAPS2.Images.Gdi;

namespace NAPS2.Modules;

public class GdiModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<GdiImageContext>().As<ImageContext>();
        builder.RegisterType<GdiImageContext>().AsSelf();
    }
}