using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NAPS2.Config;
using NAPS2.Host;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Ocr;
using NAPS2.Operation;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Scan.Twain;
using NAPS2.Scan.Wia;
using NAPS2.Update;
using NAPS2.Util;
using NAPS2.WinForms;
using Ninject.Modules;
using NLog;
using ILogger = NAPS2.Util.ILogger;

namespace NAPS2.DI.Modules
{
    public class CommonModule : NinjectModule
    {
        public override void Load()
        {
            // Import
            Bind<IScannedImageImporter>().To<ScannedImageImporter>();
            Bind<IPdfImporter>().To<PdfSharpImporter>();
            Bind<IImageImporter>().To<ImageImporter>();

            // Export
            Bind<IPdfExporter>().To<PdfSharpExporter>();
            Bind<IScannedImagePrinter>().To<PrintDocumentPrinter>();
            Bind<IEmailer>().To<MapiEmailer>();
            Bind<IOcrEngine>().To<TesseractOcrEngine>();

            // Scan
            Bind<IScanPerformer>().To<ScanPerformer>();
#if DEBUG && false
            Bind<IScanDriverFactory>().To<Scan.Stub.StubScanDriverFactory>();
#else
            Bind<IScanDriverFactory>().To<NinjectScanDriverFactory>();
#endif
            Bind<IScanDriver>().To<WiaScanDriver>().Named(WiaScanDriver.DRIVER_NAME);
            Bind<IScanDriver>().To<TwainScanDriver>().Named(TwainScanDriver.DRIVER_NAME);

            // Config
            Bind<IProfileManager>().To<ProfileManager>().InSingletonScope();
            Bind<AppConfigManager>().ToSelf().InSingletonScope();
            Bind<IUserConfigManager>().To<UserConfigManager>().InSingletonScope();
            Bind<PdfSettingsContainer>().ToSelf().InSingletonScope();
            Bind<ImageSettingsContainer>().ToSelf().InSingletonScope();
            Bind<EmailSettingsContainer>().ToSelf().InSingletonScope();

            // Update
            Bind<IAutoUpdater>().To<AutoUpdater>();
            Bind<ICurrentVersionSource>().To<CurrentVersionSource>();
            // TODO: Link to web
            Bind<ILatestVersionSource>().To<LatestVersionSource>().WithConstructorArgument("versionFileUrl", "file://" + Path.Combine(Environment.CurrentDirectory, "../../../version.xml"));
            Bind<IUrlFileDownloader>().To<UrlFileDownloader>();
            Bind<IUrlStreamReader>().To<UrlStreamReader>();
            Bind<IUrlTextReader>().To<UrlTextReader>();
            Bind<Edition>().ToConstant(GetEdition());

            // Host
            Bind<IX86HostServiceFactory>().To<NinjectX86HostServiceFactory>();
            Bind<IX86HostService>().ToMethod(ctx => X86HostManager.Connect());

            // Misc
            Bind<IFormFactory>().To<NinjectFormFactory>();
            Bind<IOperationFactory>().To<NinjectOperationFactory>();
            Bind<ILogger>().To<NLogLogger>().InSingletonScope();
            Bind<ChangeTracker>().ToSelf().InSingletonScope();
            Bind<StillImage>().ToSelf().InSingletonScope();
            Bind<IBlankDetector>().To<ThresholdBlankDetector>();
            Bind<IAutoSave>().To<AutoSave>();

            Log.Logger = new NLogLogger();
#if DEBUG
            Debug.Listeners.Add(new NLogTraceListener());
#endif
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
