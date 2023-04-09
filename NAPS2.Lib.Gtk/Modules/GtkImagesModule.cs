using Autofac;
using NAPS2.Images.Gtk;

namespace NAPS2.Modules;

public class GtkImagesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<GtkImageContext>().As<ImageContext>();
        builder.RegisterType<GtkImageContext>().AsSelf();
    }
}
