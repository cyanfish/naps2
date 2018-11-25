using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NAPS2.ClientServer;
using NAPS2.Config;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Logging;
using NAPS2.Ocr;
using NAPS2.Operation;
using NAPS2.Platform;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Scan.Sane;
using NAPS2.Scan.Twain;
using NAPS2.Scan.Wia;
using NAPS2.Util;
using NAPS2.WinForms;
using NAPS2.Worker;
using Ninject;
using Ninject.Modules;
using NLog;
using ILogger = NAPS2.Logging.ILogger;

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
            Bind<IPdfRenderer>().To<GhostscriptPdfRenderer>().InSingletonScope();

            // Export
            Bind<IPdfExporter>().To<PdfSharpExporter>();
            Bind<IScannedImagePrinter>().To<PrintDocumentPrinter>();
            Bind<IEmailProviderFactory>().To<NinjectEmailProviderFactory>();
            Bind<OcrManager>().ToSelf().InSingletonScope();
            Bind<OcrRequestQueue>().ToSelf().InSingletonScope();

            // Scan
            Bind<IScanPerformer>().To<ScanPerformer>();
#if DEBUG && false
            Bind<IScanDriverFactory>().To<Scan.Stub.StubScanDriverFactory>();
#else
            Bind<IScanDriverFactory>().To<NinjectScanDriverFactory>();
#endif
            Bind<IScanDriver>().To<WiaScanDriver>().InSingletonScope().Named(WiaScanDriver.DRIVER_NAME);
            Bind<IScanDriver>().To<TwainScanDriver>().InSingletonScope().Named(TwainScanDriver.DRIVER_NAME);
            Bind<IScanDriver>().To<SaneScanDriver>().InSingletonScope().Named(SaneScanDriver.DRIVER_NAME);
            Bind<IScanDriver>().To<ProxiedScanDriver>().InSingletonScope().Named(ProxiedScanDriver.DRIVER_NAME);

            // Config
            Bind<IProfileManager>().To<ProfileManager>().InSingletonScope();
            Bind<AppConfigManager>().ToSelf().InSingletonScope();
            Bind<IUserConfigManager>().To<UserConfigManager>().InSingletonScope();
            Bind<PdfSettingsContainer>().ToSelf().InSingletonScope();
            Bind<ImageSettingsContainer>().ToSelf().InSingletonScope();
            Bind<EmailSettingsContainer>().ToSelf().InSingletonScope();

            // Host
            Bind<IWorkerServiceFactory>().To<NinjectWorkerServiceFactory>();
            Bind<WorkerContext>().ToMethod(ctx => WorkerManager.NextWorker());

            // Misc
            Bind<IFormFactory>().To<NinjectFormFactory>();
            Bind<NotificationManager>().ToSelf().InSingletonScope();
            Bind<IOperationFactory>().To<NinjectOperationFactory>();
            Bind<ILogger>().To<NLogLogger>().InSingletonScope();
            Bind<ChangeTracker>().ToSelf().InSingletonScope();
            Bind<StillImage>().ToSelf().InSingletonScope();
            Bind<IBlankDetector>().To<ThresholdBlankDetector>();
            Bind<IAutoSave>().To<AutoSave>();

            Log.Logger = new NLogLogger();
            if (PlatformCompat.System.CanUseWin32)
            {
                Log.EventLogger = Kernel.Get<WindowsEventLogger>();
            }
#if DEBUG
            Debug.Listeners.Add(new NLogTraceListener());
#endif
        }
    }
}
