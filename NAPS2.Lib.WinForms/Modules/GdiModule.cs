using NAPS2.Images.Gdi;
using Ninject.Modules;

namespace NAPS2.Modules;

public class GdiModule : NinjectModule
{
    public override void Load()
    {
        Bind<ImageContext>().To<GdiImageContext>();
        Bind<GdiImageContext>().ToSelf();
    }
}