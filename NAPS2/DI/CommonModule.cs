using System;
using System.Collections.Generic;
using System.IO;
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
using NAPS2.Update;
using Ninject.Modules;
using NLog;

namespace NAPS2.DI
{
    public class CommonModule : NinjectModule
    {
        public override void Load()
        {
            // Import
            Bind<IScannedImageImporter>().To<ScannedImageImporter>().When(x => true); // Fix so that this binding is only used when no name is specified
            Bind<IScannedImageImporter>().To<PdfSharpImporter>().Named("pdf");
            Bind<IScannedImageImporter>().To<ImageImporter>().Named("image");

            // Export
            Bind<IPdfExporter>().To<PdfSharpExporter>();
            Bind<IEmailer>().To<MapiEmailer>();

            // Scan
            Bind<IScannedImageFactory>().To<FileBasedScannedImageFactory>();
            Bind<IScanPerformer>().To<ScanPerformer>();
#if DEBUG && false
            Bind<IScanDriver>().To<StubScanDriver>().Named(WiaScanDriver.DRIVER_NAME).WithConstructorArgument("driverName", WiaScanDriver.DRIVER_NAME);
            Bind<IScanDriver>().To<StubScanDriver>().Named(TwainScanDriver.DRIVER_NAME).WithConstructorArgument("driverName", TwainScanDriver.DRIVER_NAME);
#else
            Bind<IScanDriver>().To<WiaScanDriver>().Named(WiaScanDriver.DRIVER_NAME);
            Bind<IScanDriver>().To<TwainScanDriver>().Named(TwainScanDriver.DRIVER_NAME);
#endif

            // Config
            Bind<IProfileManager>().To<ProfileManager>().InSingletonScope();
            Bind<AppConfigManager>().ToSelf().InSingletonScope();
            Bind<UserConfigManager>().ToSelf().InSingletonScope();

            // Update
            Bind<IAutoUpdater>().To<AutoUpdater>();
            Bind<ICurrentVersionSource>().To<CurrentVersionSource>();
            // TODO: Link to web
            Bind<ILatestVersionSource>().To<LatestVersionSource>().WithConstructorArgument("versionFileUrl", "file://" + Path.Combine(Environment.CurrentDirectory, "../../../version.xml"));
            Bind<IUrlFileDownloader>().To<UrlFileDownloader>();
            Bind<IUrlStreamReader>().To<UrlStreamReader>();
            Bind<IUrlTextReader>().To<UrlTextReader>();
            Bind<Edition>().ToConstant(GetEdition());

            // Misc
            Bind<Logger>().ToMethod(ctx => LoggerFactory.Current.GetLogger()).InSingletonScope();
        }

        private Edition GetEdition()
        {
#if STANDALONE_ZIP
            return Edition.StandaloneZIP;
#elif STANDALONE_7Z
            return Edition.Standalone7Z;
#elif INSTALLER_EXE
            return Edition.InstallerEXE;
#elif INSTALLER_MSI
            return Edition.InstallerMSI;
#else // Debug
            return Edition.InstallerEXE;
#endif
        }
    }
}
