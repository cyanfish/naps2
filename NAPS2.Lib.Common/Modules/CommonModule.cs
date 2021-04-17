using System.IO;
using NAPS2.Config;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Ocr;
using NAPS2.Operation;
using NAPS2.Platform.Windows;
using NAPS2.Remoting.Worker;
using NAPS2.Scan;
using NAPS2.Scan.Batch;
using NAPS2.Scan.Internal;
using NAPS2.Util;
using NAPS2.WinForms;
using Ninject;
using Ninject.Modules;
using ILogger = NAPS2.Logging.ILogger;

namespace NAPS2.Modules
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
            Bind<PdfExporter>().To<PdfSharpExporter>();
            Bind<IScannedImagePrinter>().To<PrintDocumentPrinter>();
            Bind<IEmailProviderFactory>().To<NinjectEmailProviderFactory>();
            Bind<IMapiWrapper>().To<MapiWrapper>();
            Bind<OcrEngineManager>().ToMethod(ctx => OcrEngineManager.Default);
            Bind<OcrRequestQueue>().ToSelf().InSingletonScope();

            // Scan
            Bind<IScanPerformer>().To<ScanPerformer>();
            Bind<IBatchScanPerformer>().To<BatchScanPerformer>();
            Bind<ILocalPostProcessor>().To<LocalPostProcessor>();
            Bind<IRemotePostProcessor>().To<RemotePostProcessor>();
            Bind<IScanBridgeFactory>().To<ScanBridgeFactory>();
            Bind<IScanDriverFactory>().To<ScanDriverFactory>();
            Bind<IRemoteScanController>().To<RemoteScanController>();
            Bind<InProcScanBridge>().ToSelf();
            Bind<WorkerScanBridge>().ToSelf();
            Bind<NetworkScanBridge>().ToSelf();

            // Config
            var config = new ScopedConfig(Path.Combine(Paths.Executable, "appsettings.xml"), Path.Combine(Paths.AppData, "config.xml"));
            Bind<ScopedConfig>().ToConstant(config);
            Bind<IConfigProvider<PdfSettings>>().ToMethod(ctx => ctx.Kernel.Get<ScopedConfig>().Child(c => c.PdfSettings));
            Bind<IConfigProvider<ImageSettings>>().ToMethod(ctx => ctx.Kernel.Get<ScopedConfig>().Child(c => c.ImageSettings));
            Bind<IConfigProvider<EmailSettings>>().ToMethod(ctx => ctx.Kernel.Get<ScopedConfig>().Child(c => c.EmailSettings));
            Bind<IConfigProvider<EmailSetup>>().ToMethod(ctx => ctx.Kernel.Get<ScopedConfig>().Child(c => c.EmailSetup));

            // Host
            Bind<IWorkerFactory>().To<WorkerFactory>().InSingletonScope();

            // Misc
            Bind<IFormFactory>().To<NinjectFormFactory>();
            Bind<NotificationManager>().ToSelf().InSingletonScope();
            Bind<ISaveNotify>().ToMethod(ctx => ctx.Kernel.Get<NotificationManager>());
            Bind<IOperationFactory>().To<NinjectOperationFactory>();
            Bind<ILogger>().To<NLogLogger>().InSingletonScope();
            Bind<ScannedImageList>().ToSelf().InSingletonScope();
            Bind<StillImage>().ToSelf().InSingletonScope();
            Bind<AutoSaver>().ToSelf();
            Bind<BitmapRenderer>().ToSelf();
            Bind<ImageContext>().To<GdiImageContext>().InSingletonScope();

            Kernel.Get<ImageContext>().PdfRenderer = Kernel.Get<PdfiumWorkerCoordinator>();

            var profileManager = new ProfileManager(
                Path.Combine(Paths.AppData, "profiles.xml"),
                Path.Combine(Paths.Executable, "profiles.xml"),
                config.Get(c => c.LockSystemProfiles),
                config.Get(c => c.LockUnspecifiedDevices),
                config.Get(c => c.NoUserProfiles));
            Bind<IProfileManager>().ToConstant(profileManager);

            StaticConfiguration.Initialize(Kernel);
        }
    }
}
