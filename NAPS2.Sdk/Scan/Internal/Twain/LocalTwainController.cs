using System.Threading;
using NAPS2.Scan.Exceptions;

namespace NAPS2.Scan.Internal.Twain;

public class LocalTwainController : ITwainController
{
    public async Task StartScan(ScanOptions options, ITwainEvents twainEvents, CancellationToken cancelToken)
    {
        // TODO: An error in NTwain doesn't seem to be logged or propagated back to the parent process correctly
        // TODO: Specifically, in TwainSessionRunner.Init
        // TODO: Cancelling twain shows a cancellation error
        // TODO: There seems to be some issue with the UI getting locked; probably event-loop related, inconsistent. Unsure if related to the new NTWAIN or the new implementation or what. 
        try
        {
            await InternalScan(options.TwainOptions.Dsm, options, cancelToken, twainEvents);
        }
        catch (DeviceNotFoundException)
        {
            if (options.TwainOptions.Dsm != TwainDsm.Old)
            {
                // Fall back to OldDsm in case of no devices
                // This is primarily for Citrix support, which requires using twain_32.dll for TWAIN passthrough
                await InternalScan(TwainDsm.Old, options, cancelToken, twainEvents);
            }
            else
            {
                throw;
            }
        }
    }

    private async Task InternalScan(TwainDsm dsm, ScanOptions options, CancellationToken cancelToken, ITwainEvents twainEvents)
    {
        var runner = new TwainSessionRunner(dsm, options, cancelToken, twainEvents);
        await runner.Run();
    }
}