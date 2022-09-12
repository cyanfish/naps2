using NAPS2.Images.Gdi;
using NAPS2.ImportExport.Images;
using Ninject.Modules;

namespace NAPS2.Modules;

public class GdiModule : NinjectModule
{
    public override void Load()
    {
        Bind<ITiffHelper>().To<TiffHelper>();
        Bind<ImageContext>().To<GdiImageContext>();
        Bind<GdiImageContext>().ToSelf();
    }
}