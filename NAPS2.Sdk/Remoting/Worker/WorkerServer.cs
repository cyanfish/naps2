using System.Threading;
using GrpcDotNetNamedPipes;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Platform.Windows;
using NAPS2.Scan;
using NAPS2.Scan.Internal.Twain;

namespace NAPS2.Remoting.Worker;

/// <summary>
/// Entry point for the NAPS2.Worker.exe binary. You can use this to build your own custom binary instead of using the
/// one in the NAPS2.Sdk.Worker.Win32 nuget package.
/// </summary>
public static class WorkerServer
{
    public static async Task Run(ScanningContext scanningContext, CancellationToken cancellationToken = default)
    {
        try
        {
            var messagePump = Win32MessagePump.Create();
            messagePump.Logger = scanningContext.Logger;
            Invoker.Current = messagePump;
            TwainHandleManager.Factory = () => new Win32TwainHandleManager(messagePump);

            var server =
                new NamedPipeServer(string.Format(WorkerFactory.PIPE_NAME_FORMAT, Process.GetCurrentProcess().Id));
            var serviceImpl = new WorkerServiceImpl(scanningContext,
                new ThumbnailRenderer(scanningContext.ImageContext), new StubMapiWrapper(),
#if MAC
                new StubTwainController());
#else
                new LocalTwainController(scanningContext));
#endif
            serviceImpl.OnStop += (_, _) => messagePump.Dispose();
            WorkerService.BindService(server.ServiceBinder, serviceImpl);
            cancellationToken.Register(() => serviceImpl.Stop());
            server.Start();
            try
            {
                Console.WriteLine(@"ready");
                messagePump.RunMessageLoop();
            }
            finally
            {
                server.Kill();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(@"error");
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
}