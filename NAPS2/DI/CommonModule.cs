using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Scan.Stub;
using NAPS2.Scan.Twain;
using NAPS2.Scan.Wia;
using Ninject.Modules;
using NLog;

namespace NAPS2.DI
{
    public class CommonModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IScannedImageImporter>().To<ScannedImageImporter>().When(x => true); // Fix so that this binding is only used when no name is specified
            Bind<IScannedImageImporter>().To<PdfSharpImporter>().Named("pdf");
            Bind<IScannedImageImporter>().To<ImageImporter>().Named("image");
            Bind<IScannedImageFactory>().To<FileBasedScannedImageFactory>();
            Bind<IScanPerformer>().To<ScanPerformer>();
            Bind<IProfileManager>().To<ProfileManager>().InSingletonScope();
            Bind<AppConfigManager>().ToSelf().InSingletonScope();
            Bind<UserConfigManager>().ToSelf().InSingletonScope();
            Bind<IPdfExporter>().To<PdfSharpExporter>();
            Bind<IEmailer>().To<MapiEmailer>();
            Bind<Logger>().ToMethod(ctx => LoggerFactory.Current.GetLogger()).InSingletonScope();
#if DEBUG && false
            Bind<IScanDriver>().To<StubScanDriver>().Named(WiaScanDriver.DRIVER_NAME).WithConstructorArgument("driverName", WiaScanDriver.DRIVER_NAME);
            Bind<IScanDriver>().To<StubScanDriver>().Named(TwainScanDriver.DRIVER_NAME).WithConstructorArgument("driverName", TwainScanDriver.DRIVER_NAME);
#else
            Bind<IScanDriver>().To<WiaScanDriver>().Named(WiaScanDriver.DRIVER_NAME);
            Bind<IScanDriver>().To<TwainScanDriver>().Named(TwainScanDriver.DRIVER_NAME);
#endif
        }
    }
}
