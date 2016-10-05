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
    }
}
