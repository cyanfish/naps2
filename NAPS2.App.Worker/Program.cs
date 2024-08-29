using System.Runtime;
using NAPS2.EntryPoints;
using NAPS2.Images.Gdi;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Platform.Windows;
using NAPS2.Remoting.Worker;
using NAPS2.Scan;
using NAPS2.Scan.Internal.Twain;

namespace NAPS2.Worker;

static class Program
{
    /// <summary>
    /// The NAPS2.Worker.exe main method.
    /// </summary>
    [STAThread]
    static int Main(string[] args)
    {
        var profilesPath = Path.Combine(Paths.AppData, "jit");
        Directory.CreateDirectory(profilesPath);
        ProfileOptimization.SetProfileRoot(profilesPath);
        ProfileOptimization.StartProfile("naps2.worker.jit");

        // This NAPS2.App.Worker project doesn't follow the conventions of the rest of NAPS2 as far as using EntryPoint
        // classes for everything. The reason is that we want to avoid pulling in extra dependencies as NAPS2.Worker.exe
        // is 32-bit and therefore requires a second copy of every single dependency we use.
        //
        // Thus the simplest solution is just to pull in a bit of code from NAPS2.Lib that has what we need
        // (pretty much only paths, logging, and the worker setup) and avoid using Autofac.
        var messagePump = Win32MessagePump.Create();
        var scanningContext = new ScanningContext(new GdiImageContext());
        var logger = NLogConfig.CreateLogger(() => true);
        var serviceImpl = new WorkerServiceImpl(scanningContext, new ThumbnailRenderer(scanningContext.ImageContext),
            new MapiWrapper(new SystemEmailClients(scanningContext)), new LocalTwainController(scanningContext));

        Trace.Listeners.Add(new NLog.NLogTraceListener());
        scanningContext.Logger = logger;
        messagePump.Logger = logger;
        Invoker.Current = messagePump;
        TwainHandleManager.Factory = () => new Win32TwainHandleManager(messagePump);

        return CoreWorkerEntryPoint.Run(args, logger, serviceImpl, messagePump.RunMessageLoop, messagePump.Dispose);
    }
}