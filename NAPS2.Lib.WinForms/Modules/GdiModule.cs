using Autofac;
using NAPS2.Images.Gdi;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Scan;
using NAPS2.Scan.Twain.Legacy;

namespace NAPS2.Modules;

public class GdiModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<GdiImageContext>().As<ImageContext>();
        builder.RegisterType<GdiImageContext>().AsSelf();
        builder.RegisterType<MapiWrapper>().As<IMapiWrapper>();

        builder.RegisterBuildCallback(ctx =>
        {
            var scanningContext = ctx.Resolve<ScanningContext>();
            scanningContext.LegacyTwainDriver = new LegacyTwainScanDriver(scanningContext);
        });
    }
}