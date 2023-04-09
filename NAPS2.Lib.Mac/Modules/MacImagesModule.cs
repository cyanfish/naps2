using Autofac;
using NAPS2.Images.Mac;

namespace NAPS2.Modules;

public class MacImagesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<MacImageContext>().As<ImageContext>();
        builder.RegisterType<MacImageContext>().AsSelf();
    }
}
