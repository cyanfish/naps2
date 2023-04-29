using System.Threading;
using GrpcDotNetNamedPipes;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.Scan;
using NAPS2.Scan.Internal.Twain;

namespace NAPS2.Remoting.Worker;

public static class WorkerServer
{
    public static async Task Run(ScanningContext scanningContext, CancellationToken cancellationToken = default)
    {
        try
        {
            var tcs = new TaskCompletionSource<bool>();
            var server =
                new NamedPipeServer(string.Format(WorkerFactory.PIPE_NAME_FORMAT, Process.GetCurrentProcess().Id));
            var serviceImpl = new WorkerServiceImpl(scanningContext,
                new ThumbnailRenderer(scanningContext.ImageContext), new StubMapiWrapper(),
#if MAC
                new StubTwainSessionController());
#else
                new LocalTwainSessionController(scanningContext));
#endif
            serviceImpl.OnStop += (_, _) => tcs.SetResult(true);
            WorkerService.BindService(server.ServiceBinder, serviceImpl);
            cancellationToken.Register(() => serviceImpl.Stop());
            server.Start();
            try
            {
                Console.WriteLine(@"ready");
                await tcs.Task;
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